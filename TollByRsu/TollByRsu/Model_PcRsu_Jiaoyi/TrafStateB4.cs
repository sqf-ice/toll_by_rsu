using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model_PcRsu_Jiaoyi
{
    public class TrafStateB4 : TrafState
    {
        private KtEtcTraf _ktl;

        public TrafStateB4(KtEtcTraf ktl)
        {
            _ktl = ktl;
        }

        public override string DisplayName { get { return "traficState(B4)."; } }

        public override void StateWorker(byte[] bxFrame, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override void StateWorker()
        {
            byte[] recvBuffer = null;
            byte[] cx;

            _ktl.PcRsu_CommIO.Receive(out recvBuffer);

            if (!BxFrameCheck(recvBuffer))
            {
                //接收到无效帧,进入B2状态
                _ktl.Jiaoyi_jieguo_message = "接收到无效帧";
                return;
            }

            if (recvBuffer[1] == 0xB2 && recvBuffer[6] == 0x80)
            {
                //心跳帧
                _ktl.Jiaoyi_jieguo_message = "接收到心跳帧";
                return;
            }


            _ktl.JiaoyiData.Add(recvBuffer);

            if(recvBuffer[1] != 0xB4)
            {                                    
                _ktl.Jiaoyi = false;    //交易结束
                _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                _ktl.Jiaoyi_jieguo_message = "STATE-B4";  //交易结果的描述

                _ktl.TS = _ktl.StateB2; //进入到B2状态

                
                cx = new byte[8];
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = 0xc2;
                Array.Copy(recvBuffer, 2, cx, 2, 4);
                cx[6] = 1;
                _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
                return;
            }

            if(recvBuffer[6] != 0x00)
            {                               
                _ktl.Jiaoyi = false;    //交易结束
                _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                _ktl.Jiaoyi_jieguo_message = "STATE-B4";  //交易结果的描述

                _ktl.TS = _ktl.StateB2; //进入到B2状态
                
                cx = new byte[8];
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = 0xc2;
                Array.Copy(recvBuffer, 2, cx, 2, 4);
                cx[6] = 1;
                _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
                return; 
            }
            
            #region 判断是复合还是传统消费

                byte laneMode = _ktl.c0_LaneMode;
                byte transClass = _ktl.c0_TransClass;
                byte cmdType = 0;
                if (laneMode == 3)
                {
                    if (transClass == 0)
                    {
                        cmdType = 0xc3;
                        //msg = "chuantongjiaoyi";
                    }
                    else if (transClass == 2 && recvBuffer[26] == 23)
                    {
                        cmdType = 0xc3;
                        //msg = "jizhang chuantong jiaoyi";
                    }
                    else
                    {
                        cmdType = 0xc6;
                        //msg = "fuhe jiaoyi";
                    }
                }
                else
                {
                    cmdType = 0xc6;
                    //msg = "fuhe jiaoyi";
                }

                #endregion
                        
            #region 获取发行方

            byte[] card_network = new byte[2];
            byte[] card_divFacotr = null;
            Array.Copy(recvBuffer, 28, card_network, 0, 2);

            byte[] _card_divFactor_id = new byte[8];
            Array.Copy(recvBuffer, 18, _card_divFactor_id, 0, 8);
            int _t = _card_divFactor_id[0] + _card_divFactor_id[1] + _card_divFactor_id[2] + _card_divFactor_id[3];
            if (_t == 0)
            {   //如果RSU没有提供发行方（数据为全0，北京车道问题),那么走另一套流程确定分散因子
                if (_ktl.CardDivFactor.ContainsKey(card_network[0]))
                {
                    card_divFacotr = _ktl.CardDivFactor[card_network[0]];
                }
                else
                {
                    card_divFacotr = null;
                }
            }
            else
            {
                //如果RSU提供发行方（数据为非全0),那么走标准流程确定分散因子
                card_divFacotr = new byte[8];
                Array.Copy(_card_divFactor_id, 0, card_divFacotr, 0, 4);
                Array.Copy(_card_divFactor_id, 0, card_divFacotr, 4, 4);
            }

            #endregion

            #region 判断无效发行方

            if (card_divFacotr == null)
            { 
                //无效发行方，交易失败
                cx = new byte[8];
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = 0xc2;
                Array.Copy(recvBuffer, 2, cx, 2, 4);
                cx[6] = 1;


                _ktl.Jiaoyi = false;    //交易结束
                _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                _ktl.Jiaoyi_jieguo_message = "无效发行方:" + ViaHere.ByteArraryToHexString(card_network);  //交易结果的描述

                _ktl.TS = _ktl.StateB2; //进入到B2状态

                _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
                return; 

            }

            #endregion

            #region 判断不支持的交易类型

            if(cmdType != 0xc3 && cmdType != 0xc6)
            {
                cx = new byte[8];
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = 0xc2;
                Array.Copy(recvBuffer, 2, cx, 2, 4);
                cx[6] = 1;

                _ktl.Jiaoyi = false;    //交易结束
                _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                _ktl.Jiaoyi_jieguo_message = "不支持的交易类型";  //交易结果的描述

                _ktl.TS = _ktl.StateB2; //进入到B2状态

                _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
                return; 

            }

            #endregion

            #region 余额不足

            if (cmdType == 0xc6)
            {
                uint cardresetmoney = 0;    //卡余额
                uint consumeMoney = 0;      //应扣款
                for (int i = 0; i < 4; i++)
                {
                    cardresetmoney <<= 8;
                    cardresetmoney += recvBuffer[10 + i];

                    consumeMoney <<= 8;
                    consumeMoney += _ktl.ConsumeMoney[i];
                }


                if (cardresetmoney < consumeMoney)
                {
                    cx = new byte[15];
                    cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                    cx[1] = 0xc1;
                    Array.Copy(recvBuffer, 2, cx, 2, 4);
                    Array.Copy(recvBuffer, 7, _ktl.obuDivFactor, 0, 8);


                    _ktl.Jiaoyi = false;    //交易结束
                    _ktl.Jiaoyi_jieguo = 1; //交易结果：失败
                    _ktl.Jiaoyi_jieguo_message = "卡余额不足";  //交易结果的描述

                    _ktl.TS = _ktl.StateB2; //进入到B2状态

                    _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
                    return;
                }
            }

            #endregion

            #region 继续交易

            if (cmdType == 0xc3)
            {
                cx = new byte[62];
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = cmdType;
                Array.Copy(recvBuffer, 2, cx, 2, 4);
                Array.Copy(card_divFacotr, 0, cx, 6,8);
                Array.Copy(_ktl.TransSerial, 0, cx, 14, 4);

                string temp = DateTime.Now.ToString("yyyyMMddHHmmss");
                Array.Copy(ViaHere.HexStringToByteArray(temp), 0, cx, 18, 7);
                Array.Copy(_ktl.Station, 0, cx, 25, 36);

                _ktl.TS = _ktl.StateB5s; //进入到B5s状态
            }
            else 
            {
                cx = new byte[66];
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = cmdType;
                cx[0] = ViaHere.ConverterHL(recvBuffer[0]);
                cx[1] = cmdType;
                Array.Copy(recvBuffer, 2, cx, 2, 4);
                Array.Copy(card_divFacotr, 0, cx, 6, 8);
                Array.Copy(_ktl.TransSerial, 0, cx, 14, 4);
                Array.Copy(_ktl.ConsumeMoney, 0, cx, 18, 4);

                string temp = DateTime.Now.ToString("yyyyMMddHHmmss");
                Array.Copy(ViaHere.HexStringToByteArray(temp), 0, cx, 22, 7);
                Array.Copy(_ktl.Station, 0, cx, 29, 36);

                _ktl.TS = _ktl.StateB5; //进入到B5状态
            } 
            _ktl.PcRsu_CommIO.Send(cx, 0, cx.Length);
            _ktl.JiaoyiData.Add(cx);

            #endregion

        }
    }
}
