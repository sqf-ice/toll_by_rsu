using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model_PcRsu_Jiaoyi
{
    public class TrafStateB3 : TrafState
    {
        private KtEtcTraf _ktl;

        public TrafStateB3(KtEtcTraf ktl)
        {
            _ktl = ktl;
        }

        public override string DisplayName { get { return "traficState(B3)."; } }


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
                _ktl.PcRsu_CommIO.ReceiveTimeOut = 1000;
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
                //接收到无效帧
                return;
            }
            if (recvBuffer[1] == 0xB2 && recvBuffer[6] == 0x80)
            {
                //心跳帧
                return;
            }

            
            _ktl.JiaoyiData.Add(recvBuffer);

            if (recvBuffer[1] == 0xB3 && recvBuffer[6] == 0x0)
            {
                //组织C1
                c1[0] = ViaHere.ConverterHL(recvBuffer[0]);
                c1[1] = 0xc1;
                Array.Copy(recvBuffer, 2, c1, 2, 4);
                Array.Copy(_ktl.obuDivFactor,0, c1, 6, 8);

                //进入B4状态
                _ktl.TS = _ktl.StateB4;
            }
            else
            {
                //进入到B2状态
                _ktl.TS = _ktl.StateB2;

                //交易失败
                _ktl.Jiaoyi = false;    //交易结束
                _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                _ktl.Jiaoyi_jieguo_message = "STATE-B3";  //交易结果的描述

            }

            _ktl.PcRsu_CommIO.Send(c1, 0, c1.Length);
            _ktl.JiaoyiData.Add(c1);
        }
    }
}
