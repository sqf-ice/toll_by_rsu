using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model_PcRsu_Jiaoyi
{
    /// <summary>
    /// 交易状态
    /// fanshiming 201208
    /// </summary>
    public abstract class TrafState
    {
        protected TrafState() { }

        public abstract void StateWorker(
            byte[] bxFrame,     //来自RSU的BX帧
            int index,          //bxFrame的起始索引位置
            int count           //bxFrame的长度
            );

        public abstract void StateWorker(); //自动使用RSUCOMMIO句柄接收和发送数据

        public virtual string DisplayName
        {
            get
            {
                return "traficState(default name).";
            }
            private set { }
        }

        #region 数据帧格式检查 BXFRAME CHECK

        /// <summary>
        /// 返回FALSE，表示该帧无效
        /// </summary>
        /// <param name="bx"></param>
        /// <returns></returns>
        protected bool BxFrameCheck(byte[] bx)
        {
            if (bx == null) return false;
            if (bx.Length <= 2) return false;

            bool rt = true;

            //帧长度以及帧头标识的检查
            switch (bx[1])
            {
                case 0xB0:
                    if (bx.Length != 29 && bx.Length != 28) { rt = false; }
                    break;
                case 0xB1:
                    if (bx.Length != 5) { rt = false; }
                    break;
                case 0xB2:
                    if (bx.Length >= 7 && bx[6] == 0x80) { break; }
                    else if (bx.Length != 35) { rt = false; }
                    break;
                case 0xB3:
                    if (bx.Length != 24) { rt = false; }
                    break;
                case 0xB4:
                    if (bx.Length != 98 && bx.Length != 101) { rt = false; }
                    break;
                case 0xB5:
                    if (bx.Length != 18 && bx.Length != 40
                        && bx.Length < 8 //仅检查到status字段
                        ) { rt = false; }
                    break;
                default:
                    rt = false;
                    break;
            }

            //BCC CHECK
            if (bx[bx.Length - 1] != ViaHere.BccCalc(bx, 0, bx.Length - 1))
            { rt = false; }

            return rt;
        }

        #endregion
    }
}
