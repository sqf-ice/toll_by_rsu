using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model_PcRsu_Jiaoyi
{
    public class TrafStateB0 : TrafState
    {
        private KtEtcTraf _ktl;

        public TrafStateB0(KtEtcTraf ktl)
        {
            _ktl = ktl;
        }

        public override void StateWorker(byte[] bxFrame, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override void StateWorker()
        {
            byte[] recvBuffer = null;

            try
            {
                _ktl.PcRsu_CommIO.ReceiveTimeOut = 2000;
                _ktl.PcRsu_CommIO.SendTimeOut = 120;

                byte[] c0 = CreateDefaultC0();
                c0[13] = _ktl.c0_LaneMode;
                c0[14] = _ktl.c0_WaitTime;
                c0[15] = _ktl.c0_TxPower;
                c0[16] = _ktl.c0_PLLChannelID;
                c0[17] = _ktl.c0_TransClass;

                _ktl.PcRsu_CommIO.ClearReceiveBuffer();
                _ktl.PcRsu_CommIO.Send(c0, 0, c0.Length);
                _ktl.PcRsu_CommIO.Receive(out recvBuffer);

                if (!BxFrameCheck(recvBuffer))
                {
                    //接收到无效帧,进入B0状态
                    _ktl.TS = _ktl.StateB0;

                    _ktl.Jiaoyi_jieguo_message = "初始化交易参数（接收到无效帧）:" + ViaHere.ByteArraryToHexString(recvBuffer);

                    return;
                }

                //_ktl.JiaoyiData.Clear();
                _ktl.JiaoyiData.Add(recvBuffer);

                if (recvBuffer[1] == 0xB2 && recvBuffer[6] == 0x80)
                {
                    //心跳帧
                    return;
                }

                if (recvBuffer[1] == 0xB0 
                    && recvBuffer[2] == 0x0
                    )
                {
                    //发送C1 指示继续交易
                    byte[] c1 = new byte[15];
                    c1[0] = ViaHere.ConverterHL(recvBuffer[0]);
                    c1[1] = 0xc1;
                    _ktl.PcRsu_CommIO.Send(c1, 0, c1.Length);

                    //进入B2状态
                    _ktl.TS = _ktl.StateB2;

                    _ktl.Jiaoyi_jieguo_message = "初始化交易参数OK";
                }
                else
                {
                    _ktl.Jiaoyi = false;    //交易结束
                    _ktl.Jiaoyi_jieguo = 3; //交易结果：初始化RSU失败
                    _ktl.Jiaoyi_jieguo_message = "STATE-B0";  //交易结果的描述

                    _ktl.TS = _ktl.StateB0;
                    return;
                }
            }
            catch (Exception ex) 
            {
                _ktl.Jiaoyi_jieguo_message = "初始化交易参数异常" + ex.Message;
                throw ex;
            }
            finally {
            }




        }

        public override string DisplayName{get{return "traficState(B0).";}}


        #region private helper

        private byte[] CreateDefaultC0()
        {
            byte[] _buff = new byte[19];

            _buff[0] = 0x89;
            _buff[1] = 0xc0;

            System.DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            System.DateTime _dateTime = System.DateTime.Now;
            System.TimeSpan ts = _dateTime - dt;

            int t = (int)ts.TotalSeconds;
            _buff[2] = (byte)((t & 0xff000000) >> 32);
            _buff[3] = (byte)((t & 0x00ff0000) >> 16);
            _buff[4] = (byte)((t & 0x0000ff00) >> 8);
            _buff[5] = (byte)((t & 0x000000ff));


            string temp = _dateTime.ToString("yyyyMMddHHmmss");
            Array.Copy(ViaHere.HexStringToByteArray(temp), 0, _buff, 6, 7);


            _buff[13] = 0XFF;
            _buff[14] = 0XFF;
            _buff[15] = 0XFF;
            _buff[16] = 0XFF;
            _buff[17] = 0XFF;

            return _buff;
        }


        #endregion
    }
}
