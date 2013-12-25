using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model_PcRsu_Jiaoyi
{
    public class TrafStateB5 : TrafState
    {
        private KtEtcTraf _ktl;

        public TrafStateB5(KtEtcTraf ktl)
        {
            _ktl = ktl;
        }

        public override string DisplayName { get { return "traficState(B5)."; } }

        public override void StateWorker(byte[] bxFrame, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override void StateWorker()
        {
            byte[] recvBuffer = null;
            byte[] cx;

            try
            {

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

            if (recvBuffer[1] != 0xB5)
            {
                _ktl.Jiaoyi = false;    //交易结束
                _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                _ktl.Jiaoyi_jieguo_message = "B5(接收到不是B5帧)";  //交易结果的描述

                _ktl.TS = _ktl.StateB2; //进入到B2状态


                cx = new byte[8];
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = 0xc2;
                Array.Copy(recvBuffer, 2, cx, 2, 4);
                cx[6] = 1;
                _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
                return;
            }

            if (recvBuffer[6] != 0x00)
            {
                _ktl.Jiaoyi = false;    //交易结束
                _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                _ktl.Jiaoyi_jieguo_message = "B5(帧状态异常)";  //交易结果的描述

                _ktl.TS = _ktl.StateB2; //进入到B2状态

                cx = new byte[8];
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = 0xc2;
                Array.Copy(recvBuffer, 2, cx, 2, 4);
                cx[6] = 1;
                _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
                return;
            }

            #region 交易成功

            cx = new byte[15];
            cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
            cx[1] = 0xc1;
            Array.Copy(recvBuffer, 2, cx, 2, 4);
            Array.Copy(recvBuffer, 7, _ktl.obuDivFactor, 0, 8);

            _ktl.Jiaoyi = false;    //交易结束
            _ktl.Jiaoyi_jieguo = 2; //交易结果：成功
            _ktl.Jiaoyi_jieguo_message = "交易成功";  //交易结果的描述

            _ktl.TS = _ktl.StateB2; //进入到B2状态

            _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
            _ktl.JiaoyiData.Add(cx);

            #endregion
        }
    }
}
