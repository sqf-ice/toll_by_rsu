using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model_PcRsu_Jiaoyi
{
    public class TrafStateB2 : TrafState
    {
        private KtEtcTraf _ktl;

        public TrafStateB2(KtEtcTraf ktl)
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
            byte[] c1 = new byte[15];

            try
            {
                //放开这里的注释，可以增加交易稳定性
                //if (!_ktl.PcRsu_CommIO.IsConn)
                //{
                //    _ktl.PcRsu_CommIO.Conn();
                //}

                _ktl.PcRsu_CommIO.ReceiveTimeOut = -1;
                _ktl.PcRsu_CommIO.SendTimeOut = 120;

                _ktl.PcRsu_CommIO.Receive(out recvBuffer);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }


            if (!BxFrameCheck(recvBuffer))
            {
                //接收到无效帧,进入B2状态
                return;
            }

            if (recvBuffer[1] == 0xB2 && recvBuffer[6] == 0x80)
            { 
                //心跳帧
                return;
            }

            if (recvBuffer[1] == 0xb0)
            {
                //进入B0状态
                _ktl.TS = _ktl.StateB0;
                return;
            }

            if (recvBuffer[1] != 0xB2)
            {
                return;
            }
            
            _ktl.Jiaoyi_jieguo = 0; //开始交易
            _ktl.Jiaoyi_jieguo_message = "开始交易";
            _ktl.JiaoyiData.Clear();
            _ktl.JiaoyiData.Add(recvBuffer);

            if (recvBuffer[1] == 0xB2 && recvBuffer[6] == 0x0)
            {
                //组织C1
                c1[0] = ViaHere.ConverterHL(recvBuffer[0]);
                c1[1] = 0xc1;
                Array.Copy(recvBuffer, 2, c1, 2, 4);
                Array.Copy(recvBuffer, 7, c1, 6, 8);

                Array.Copy(recvBuffer, 7, _ktl.obuDivFactor, 0, 8);

                //OBUstatus 检查无卡 锁 卡出错状态
                byte obustatusMask = 0x8c;
                byte obustatusA = recvBuffer[32];
                int iM = obustatusA & obustatusMask;
                if (iM == 0)
                {
                    //进入B3状态
                    _ktl.TS = _ktl.StateB3;
                }
                else
                {
                    //进入到B2状态
                    _ktl.TS = _ktl.StateB2;

                    //交易失败：标签无卡,锁 卡错 等等
                    _ktl.Jiaoyi = false;    //交易结束
                    _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                    _ktl.Jiaoyi_jieguo_message = "OBU状态";  //交易结果的描述
                }
            }
            else
            {
                _ktl.Jiaoyi = false;    //交易结束
                _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                _ktl.Jiaoyi_jieguo_message = "STATE-B2";  //交易结果的描述

                _ktl.TS = _ktl.StateB2; //进入到B2状态

            }

            _ktl.PcRsu_CommIO.Send(c1, 0, c1.Length);
            _ktl.JiaoyiData.Add(c1);
        }

        public override string DisplayName { get { return "traficState(B2)."; } }
    }
}
