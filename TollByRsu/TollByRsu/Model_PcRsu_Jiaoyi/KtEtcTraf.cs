using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using TollByRsu.Model;

namespace TollByRsu.Model_PcRsu_Jiaoyi
{
    public class KtEtcTraf
    {
        #region constructor

        public KtEtcTraf()
        {
            ReSetParam();


                        //一级分散因子
            CardDivFactor.Add(0x11, new byte[] { 0xB1, 0XB1, 0XBE, 0XA9, 0xB1, 0XB1, 0XBE, 0XA9});
            CardDivFactor.Add(0x12, new byte[] { 0xCC, 0XEC, 0XBD, 0XF2, 0xCC, 0XEC, 0XBD, 0XF2 });
            CardDivFactor.Add(0x13, new byte[] { 0xBA, 0XD3, 0XB1, 0XB1, 0xBA, 0XD3, 0XB1, 0XB1 });
            CardDivFactor.Add(0x14, new byte[] { 0xC9, 0XBD, 0XCE, 0XF7, 0xC9, 0XBD, 0XCE, 0XF7 });
            CardDivFactor.Add(0x21, new byte[] { 0xC1, 0XC9, 0XC4, 0XFE, 0xC1, 0XC9, 0XC4, 0XFE });
            CardDivFactor.Add(0x22, new byte[] { 0xBC, 0XAA, 0XC1, 0XD6, 0xBC, 0XAA, 0XC1, 0XD6 });
            CardDivFactor.Add(0x31, new byte[] { 0xC9, 0XCF, 0XBA, 0XA3, 0xC9, 0XCF, 0XBA, 0XA3 });


        }

        ~KtEtcTraf() { }

        #endregion

        #region rsu commio fields

        public void SetRsuCommIO(PcRsu_CommIo_Type pr)
        {
            switch (pr)
            { 
                case PcRsu_CommIo_Type.Serial:
                    CommIO_PcRsu_Serial prs = new CommIO_PcRsu_Serial();
                    prs.Sp.PortName = _serialPortName;
                    _pr = prs;
                    _pcrsu_commio_type = PcRsu_CommIo_Type.Serial;
                    break;
                case PcRsu_CommIo_Type.Tcp:
                    CommIO_PcRsu_Tcp pr_t = new CommIO_PcRsu_Tcp();
                    pr_t.ThePort = _TheTcpPort;
                    pr_t.TheIpAddress = _TheIpAddress;
                    _pr = pr_t;
                    _pcrsu_commio_type = PcRsu_CommIo_Type.Tcp;
                    break;
                case PcRsu_CommIo_Type.UN:
                default:
                    CommIO_PcRsu_Serial prs1 = new CommIO_PcRsu_Serial();
                    prs1.Sp.PortName = "COM1";
                    _pr = prs1;
                    _pcrsu_commio_type = PcRsu_CommIo_Type.UN;
                    break;
            }
        }

        CommIO_PcRsu _pr = null;
        public CommIO_PcRsu PcRsu_CommIO
        {
            get { return _pr; }
        }

        PcRsu_CommIo_Type _pcrsu_commio_type = PcRsu_CommIo_Type.UN;
        public PcRsu_CommIo_Type PcRsu_CommIoType
        {
            get { return _pcrsu_commio_type; }
            set { PcRsu_CommIoType = value; }
        }

        public enum PcRsu_CommIo_Type { UN, Serial, Tcp }

        private string _serialPortName = "COM1";
        public string serialPortName
        {
            get { return _serialPortName; }
            set { _serialPortName = value; }
        }

        private IPAddress _TheIpAddress = IPAddress.Parse("192.168.9.159");
        public IPAddress TheIpAddress
        {
            get { return _TheIpAddress; }
            set { if (value == _TheIpAddress) return; _TheIpAddress = value; }
        }

        private int _TheTcpPort = 21003;
        public int TheTcpPort
        {
            get { return _TheTcpPort; }
            set { if (value == _TheTcpPort) return; _TheTcpPort = value; }
        }

        #endregion

        #region 初始化RSU的工作参数

        public byte c0_LaneMode = 6;
        public byte c0_WaitTime = 1;
        public byte c0_TxPower = 0x0a;
        public byte c0_PLLChannelID = 0;
        public byte c0_TransClass = 1;

        public int InitC0(
            byte laneMode, byte waitTime, byte txPower, byte pllChannelID, byte transClass
            )
        {
            #region 保存C0

            c0_LaneMode = laneMode;
            c0_WaitTime = waitTime;
            c0_TxPower = txPower;
            c0_PLLChannelID = pllChannelID;
            c0_TransClass = transClass;

            #endregion

            return 0;
        }

        //车道信息
        public byte[] Station = new byte[36];

        //扣款额
        public byte[] ConsumeMoney = new byte[4] { 0, 0, 0, 1 };

        //交易顺序号
        public byte[] TransSerial = new byte[4] { 0, 0, 0, 1 };

        /// <summary>
        /// 卡分散因子。根据卡网络编号查询. exp: key=11,value=b1b1bea9b1b1bea9
        /// </summary>
        public Dictionary<byte,byte[]> CardDivFactor = new Dictionary<byte,byte[]>();

        //记住每次交易中的标签分散因子
        public byte[] obuDivFactor = new byte[8];

        //记住每次交易的所有数据
        public List<byte[]> JiaoyiData = new List<byte[]>();

        
        #endregion

        #region 交易一次
        public bool Jiaoyi = true;    //false交易结束

        /// <summary>
        /// -1 未开始交易 0开始交易（收到B2） 2成功  1失败
        /// </summary>
        public int Jiaoyi_jieguo = -1;

        public string Jiaoyi_jieguo_message = "";

         /// <summary>
        /// 连接RSU，交易一次，断开连接
        /// </summary>
        public void WorkerHere()
        {
            try
            {
                if (!PcRsu_CommIO.IsConn)
                {
                    PcRsu_CommIO.Conn();
                }

                Jiaoyi = true;
                while (Jiaoyi)
                {
                    TS.StateWorker();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (PcRsu_CommIO.IsConn) { PcRsu_CommIO.DisConn(); }
            }
        }

        #endregion


        #region 交易状态控制


        /// <summary>
        /// 交易信息 包含标签 车辆 卡 交易详细信息
        /// </summary>
        //public KtTrafInfo TrafInfo;

        private TrafState _ts;
        private TrafStateB0 _tsB0;
        private TrafStateB2 _tsB2;
        private TrafStateB3 _tsB3;
        private TrafStateB4 _tsB4;
        private TrafStateB5 _tsB5;
        private TrafStateB5s _tsB5s; //没有消费信息

        public void ReSetParam()
        {
            //TrafInfo = new KtTrafInfo();

            _tsB0 = new TrafStateB0(this);
            _tsB2 = new TrafStateB2(this);
            _tsB3 = new TrafStateB3(this);
            _tsB4 = new TrafStateB4(this);
            _tsB5 = new TrafStateB5(this);
            _tsB5s = new TrafStateB5s(this);

            _ts = _tsB0;
        }


        public string DisplayName
        {
            get
            {
                return TS.DisplayName;
            }
            private set { }
        }

        public TrafState TS 
        { 
            get { return _ts; } 
            set 
            { 
                _ts = value;
            } 
        }

        public TrafState StateB0
        {
            get
            {
                return _tsB0;
            }
        } 

        public TrafState StateB2
        {
            get
            {
                return _tsB2;
            }
        }


        public TrafState StateB3
        {
            get
            {
                return _tsB3;
            }
        }


        public TrafState StateB4
        {
            get
            {
                return _tsB4;
            }
        }


        public TrafState StateB5
        {
            get
            {
                return _tsB5;
            }
        }


        public TrafState StateB5s
        {
            get
            {
                return _tsB5s;
            }
        }




        #endregion

        #region 数据帧格式检查 BXFRAME CHECK

        /// <summary>
        /// 返回FALSE，表示该帧无效
        /// </summary>
        /// <param name="bx"></param>
        /// <returns></returns>
        public bool BxFrameCheck(byte[] bx)
        {
            if (bx == null) return false;
            if (bx.Length <= 2) return false;

            bool rt = true;

            //帧长度以及帧头标识的检查
            switch (bx[1])
            { 
                case 0xB0:
                    if (bx.Length != 29 && bx.Length != 28){rt = false;}
                    break;
                case 0xB1:
                    if (bx.Length != 5 ) { rt = false; }
                    break;
                case 0xB2:
                    if (bx.Length != 35) { rt = false; }
                    break;
                case 0xB3:
                    if (bx.Length != 24) { rt = false; }
                    break;
                case 0xB4:
                    if (bx.Length != 98 && bx.Length != 101) { rt = false; }
                    break;
                case 0xB5:
                    if (bx.Length != 18 && bx.Length != 40) { rt = false; }
                    break;
                default:
                    rt = false;
                    break;
            }

            //BCC CHECK
            

            return rt;
        }

        #endregion



    }
}
