using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TollByRsu.Model_PcRsu_Jiaoyi;
using TollByRsu.Model;

namespace TollByRsu
{
    public class PcRsu
    {
        #region constructor

        #endregion

        #region 交易控制数据

        /// <summary>
        /// 交易控制类
        /// </summary>
        public readonly KtEtcTraf ktLane = new KtEtcTraf();


        /// <summary>
        /// 每次交易时的PC-RSU数据
        /// </summary>
        public List<byte[]> pcrsu_data
        {
            get { return ktLane.JiaoyiData; }
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

        public byte c0_LaneMode 
        { get { return ktLane.c0_LaneMode; } set { ktLane.c0_LaneMode = value; } }
        public byte c0_WaitTime
        { get { return ktLane.c0_WaitTime; } set { ktLane.c0_WaitTime = value; } }
        public byte c0_TxPower
        { get { return ktLane.c0_TxPower; } set { ktLane.c0_TxPower = value; } }
        public byte c0_PLLChannelID
        { get { return ktLane.c0_PLLChannelID; } set { ktLane.c0_PLLChannelID = value; } }
        public byte c0_TransClass
        { get { return ktLane.c0_TransClass; } set { ktLane.c0_TransClass = value; } }


        #endregion

        /// <summary>
        /// 连接串口RSU
        /// </summary>
        /// <param name="serialPortName"></param>
        public void ConnectRsu(string serialPortName)
        {
            if(ktLane.PcRsu_CommIO != null && ktLane.PcRsu_CommIO.IsConn)
            {
                throw new Exception("the rsu is already connected");
            }

            ktLane.serialPortName = serialPortName;
            ktLane.SetRsuCommIO(KtEtcTraf.PcRsu_CommIo_Type.Serial);

            ktLane.PcRsu_CommIO.Conn();
        }

        /// <summary>
        /// 连接网口RSU
        /// </summary>
        /// <param name="ipAddress">点分十进制IP字符串</param>
        /// <param name="port">TCP port</param>
        public void ConnectRsu(string ipAddress, int port)
        {
            if (ktLane.PcRsu_CommIO != null && ktLane.PcRsu_CommIO.IsConn)
            {
                throw new Exception("the rsu is already connected");
            }

            ktLane.TheIpAddress = System.Net.IPAddress.Parse(ipAddress);
            ktLane.TheTcpPort = port;
            ktLane.SetRsuCommIO(KtEtcTraf.PcRsu_CommIo_Type.Serial);

            ktLane.PcRsu_CommIO.Conn();

        }

        public void DisConnectRsu()
        {
            if (ktLane.PcRsu_CommIO != null && ktLane.PcRsu_CommIO.IsConn)
            {
                ktLane.PcRsu_CommIO.DisConn();
            }
        }

        public void Jiaoyi()
        {
            try
            {
                ktLane.Jiaoyi = true;
                ktLane.PcRsu_CommIO.ClearReceiveBuffer();
                while (ktLane.Jiaoyi)
                {
                    ktLane.TS.StateWorker();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally { }
        }
    }
}
