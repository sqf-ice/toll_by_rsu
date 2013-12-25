using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model
{
    /// <summary>
    /// PC-RSU的通信接口
    /// </summary>
    public abstract class CommIO_PcRsu
    {
        ~CommIO_PcRsu()
        {
            if (IsConn)
            {
                DisConn();
            }
        }

        /// <summary>
        /// 连接数据接口
        /// </summary>
        public abstract void Conn();

        /// <summary>
        /// 断开与数据接口的连接
        /// </summary>
        public abstract void DisConn();

        /// <summary>
        /// 是否已经连接RSU
        /// </summary>
        public abstract bool IsConn { get; }

        /// <summary>
        /// 发送超时 毫秒
        /// </summary>
        public abstract int SendTimeOut { get; set; }

        /// <summary>
        /// 接收超时 毫秒
        /// </summary>
        public abstract int ReceiveTimeOut { get; set; }

        public abstract void ClearReceiveBuffer();

        /// <summary>
        /// 从RSU接收数据,整数据帧返回
        /// </summary>
        /// <param name="buffer"> Byte 类型的数组，它是存储接收到的数据的位置。</param>
        /// <returns>接收到的字节数</returns>
        public abstract void Receive(out byte[] buffer);

        /// <summary>
        /// PC向RSU发送数据
        /// </summary>
        /// <param name="sendBuff">发送缓冲区</param>
        /// <param name="offSet">偏移量</param>
        /// <param name="count">字节数量</param>       
        public abstract void Send(byte[] sendBuff, int index, int count);

        #region pc-rsu frame up

        /// <summary>
        /// 向上解封装PC-RSU数据帧,返回的是RSCTL||DATA||BCC
        /// </summary>
        /// <param name="src">PC-RSU数据帧,STX||RSCTL||DATA||BCC||ETX</param>
        /// <param name="offSet">起始位置</param>
        /// <param name="size">长度</param>
        /// <param name="dest">返回的是RSCTL||DATA||BCC</param>
        protected void PcRsuFrameUp(byte[] src, int index, int count, out byte[] dest)
        {
            if (count <= 3)
            {
                dest = null;
                return;
            }

            if (src[index + 0] != 0xff || src[index + 1] != 0xff || src[index + count - 1] != 0xff)
            {
                dest = null;
                return;
            }



            int index_rsctl = index + 2;
            int index_bcc = index + count - 1;


            for (; index_bcc >= index; index_bcc--)
            {
                if (src[index_bcc] != 0xff) { break; }
            }

            for (index_rsctl = index_bcc; index_rsctl >= index + 1; index_rsctl--)
            {
                if (src[index_rsctl - 1] == 0xff)
                {
                    break;
                }
            }

            if (index_bcc <= index_rsctl)
            {
                dest = null;
                return;
            }


            List<byte> _pcrsu_frame_up_temp = new List<byte>();
            byte tb = 0xff;
            for (int i = index_bcc; i >= index_rsctl; i--)
            {
                byte b = src[i];
                switch (b)
                {
                    case 0xfe:
                        if (tb == 0x00)
                        {
                            _pcrsu_frame_up_temp[0] = 0xfe;
                        }
                        else if (tb == 0x01)
                        {
                            _pcrsu_frame_up_temp[0] = 0xff;
                        }
                        else
                        {
                            dest = null;
                            return;
                        }
                        break;
                    default:
                        _pcrsu_frame_up_temp.Insert(0, b);
                        tb = b;
                        break;
                }
            }

            dest = _pcrsu_frame_up_temp.ToArray();

            //BCC校验 
            //if (dest[dest.Length - 1] != ViaHere.BccCalc(dest, 0, dest.Length - 1))
            //{ 
            //    dest = null;
            //}
        }


        #endregion

        #region pc-rsu frame down

        /// <summary>
        /// 向下封装PC-RSU数据帧,返回的是FF转义后的STX||RSCTL||DATA||BCC||ETX
        /// </summary>
        /// <param name="src">PC-RSU数据帧,RSCTL||DATA||BCC</param>
        /// <param name="offSet">起始位置</param>
        /// <param name="size">长度</param>
        /// <param name="dest">返回的是FF转义后的STX||RSCTL||DATA||BCC||ETX</param>
        protected void PcRsuFrameDown(byte[] src, int offSet, int size, out byte[] dest)
        {
            List<byte> _pcrsu_frame_down_temp = new List<byte>();
            _pcrsu_frame_down_temp.Add(0XFF);
            _pcrsu_frame_down_temp.Add(0XFF);

            //矫正BCC
            src[offSet + size - 1] = ViaHere.BccCalc(src, offSet, size - 1);

            for (int i = offSet; i < offSet + size; i++)
            {
                switch (src[i])
                {
                    case 0xFF:
                        _pcrsu_frame_down_temp.Add(0xfe);
                        _pcrsu_frame_down_temp.Add(0x01);
                        break;
                    case 0xFE:
                        _pcrsu_frame_down_temp.Add(0xfe);
                        _pcrsu_frame_down_temp.Add(0x00);
                        break;
                    default:
                        _pcrsu_frame_down_temp.Add(src[i]);
                        break;
                }
            }

            _pcrsu_frame_down_temp.Add(0XFF);
            dest = _pcrsu_frame_down_temp.ToArray();
        }

        #endregion
    }
}
