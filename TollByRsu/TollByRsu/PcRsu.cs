using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TollByRsu.Model_PcRsu_Jiaoyi;
using TollByRsu.Model;

namespace TollByRsu
{
    /// <summary>
    /// 用于控制RSU进行车道交易
    /// 
    /// author:fanshiming 2013 copyright(c)
    /// </summary>
    public class PcRsu
    {
        #region constructor

        #endregion

        #region 交易控制数据

        /// <summary>
        /// 交易控制类
        /// </summary>
        private readonly KtEtcTraf ktLane = new KtEtcTraf();


        /// <summary>
        /// 每次交易时的PC-RSU数据
        /// </summary>
        public List<byte[]> pcrsu_data
        {
            get { return ktLane.JiaoyiData; }
        }

        /// <summary>
        /// 交易结果。含义：-1 未开始交易 0开始交易（收到B2） 2成功  1交易失败 3初始化RSU失败
        /// </summary>
        public int jiaoyi_rt_id
        {
            get { return ktLane.Jiaoyi_jieguo; }
        }

        /// <summary>
        /// 交易结果描述。
        /// </summary>
        public string jiaoyi_rt_message
        {
            get { return ktLane.Jiaoyi_jieguo_message; }
        }

        #endregion

        #region fields

        /// <summary>
        /// 每次交易的扣款额
        /// </summary>
        public int ConsumeMoney
        {
            get 
            {
                int c = 0;
                for(int i = 0; i < ktLane.ConsumeMoney.Length; i++)
                {
                    c += ktLane.ConsumeMoney[i];
                    c <<= 8;
                }
                return c;
            }

            set 
            {
                int cm = value;
                int len = ktLane.ConsumeMoney.Length;
                for (int i = 0; i < len; i++)
                {
                    ktLane.ConsumeMoney[len - 1 - i] = (byte)((cm >> (i * 8)) & 0xff);
                }
            }
        }

        /// <summary>
        /// 车道（19文件）内容
        /// </summary>
        public byte[] StationInfo
        {
            get
            {
                byte[] t = new byte[ktLane.Station.Length];
                Array.Copy(ktLane.Station, t, t.Length);

                return t;
            }
            set
            {
                byte[] t = value;
                if (t.Length != ktLane.Station.Length) return;
                Array.Copy(t, ktLane.Station, t.Length);
            }
        }

        /// <summary>
        /// C0帧规范：车道交易模式，3 ETC入口， 4 ETC出口， 6 ETC开放式
        /// </summary>
        public byte c0_LaneMode 
        { get { return ktLane.c0_LaneMode; } 
            set {
                if (value != 3 && value != 4 && value != 6) 
                    throw new Exception("only [3,4,6] is suported");
                ktLane.c0_LaneMode = value; } }
        
        /// <summary>
        /// C0帧规范：最小重读时间，一般设置为1,2两种值，单位为秒。
        /// </summary>
        public byte c0_WaitTime
        { get { return ktLane.c0_WaitTime; } set { ktLane.c0_WaitTime = value; } }
        
        /// <summary>
        /// RSU的发射功率，1-31个级别，一般8级保证可用，车道上设置为21.
        /// </summary>
        public byte c0_TxPower
        { get { return ktLane.c0_TxPower; } 
            set {
                if (value >= 32) throw new Exception("only [0...31] is suported");
                ktLane.c0_TxPower = value; } }
        
        /// <summary>
        /// RSU与OBU通信信道，只能设置为 0或者1
        /// </summary>
        public byte c0_PLLChannelID
        { get { return ktLane.c0_PLLChannelID; } 
            set {
                if (value != 0 && value != 1) throw new Exception("only [0,1] is suported");
                ktLane.c0_PLLChannelID = value; } 
        }

        /// <summary>
        /// 交易类型。 0 全都是传统消费， 1 全都是复合消费， 2 记账卡传统消费储值卡复合消费
        /// </summary>
        public byte c0_TransClass
        { get { return ktLane.c0_TransClass; } 
            set {
                if (value != 0 && value != 1 && value != 2) 
                    throw new Exception("only [0,1,2] is suported");
                ktLane.c0_TransClass = value; } }

        /// <summary>
        /// 当前与RSU的连接方式
        /// </summary>
        public string DisplayConnect { get { return ktLane.PcRsu_CommIO.DisplayName; } }

        /// <summary>
        /// 当前的RSU连接状态
        /// </summary>
        public bool IsRsuConnected
        { get {
            return ktLane.PcRsu_CommIO.IsConn; } }

        #endregion

        /// <summary>
        /// 连接串口RSU
        /// </summary>
        /// <param name="serialPortName">串口名称</param>
        public void ConnectRsu(string serialPortName)
        {
            //if(ktLane.PcRsu_CommIO != null && ktLane.PcRsu_CommIO.IsConn)
            //{
            //    throw new Exception("the rsu is already connected");
            //}

            ktLane.serialPortName = serialPortName;
            ktLane.SetRsuCommIO(KtEtcTraf.PcRsu_CommIo_Type.Serial);

            ktLane.PcRsu_CommIO.Conn();
        }

        /// <summary>
        /// 连接网口RSU
        /// </summary>
        /// <param name="ipAddress">点分十进制IP字符串</param>
        /// <param name="port">端口号</param>
        public void ConnectRsu(string ipAddress, int port)
        {
            //if (ktLane.PcRsu_CommIO != null && ktLane.PcRsu_CommIO.IsConn)
            //{
            //    throw new Exception("the rsu is already connected");
            //}

            ktLane.TheIpAddress = System.Net.IPAddress.Parse(ipAddress);
            ktLane.TheTcpPort = port;
            ktLane.SetRsuCommIO(KtEtcTraf.PcRsu_CommIo_Type.Tcp);

            ktLane.PcRsu_CommIO.Conn();

        }

        /// <summary>
        /// 断开RSU连接
        /// </summary>
        public void DisConnectRsu()
        {
                ktLane.PcRsu_CommIO.DisConn();
        }

        /// <summary>
        /// 进行一次交易
        /// </summary>
        public void Jiaoyi()
        {
            ktLane.Jiaoyi = true;
            ktLane.PcRsu_CommIO.ClearReceiveBuffer();
            while (ktLane.Jiaoyi)
            {
                ktLane.TS.StateWorker();
            }
        }
    }
}
