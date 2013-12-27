using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model
{
    public class CommIO_PcRsu_Serial : CommIO_PcRsu
    {
        #region constructor

        public CommIO_PcRsu_Serial()
        {            
            {
                _sp = new SerialPort();

                _sp.PortName = "COM1";

                _sp.BaudRate = 115200;
                _sp.Parity = Parity.None;
                _sp.StopBits = StopBits.One;
                _sp.DataBits = 8;
                _sp.ReadTimeout = 500;
                _sp.WriteTimeout = 500;

            }

        }

        #endregion

        #region field

        private SerialPort _sp;
        public SerialPort Sp
        {
            get
            {
                if (_sp == null)
                {
                    _sp = new SerialPort();

                    _sp.PortName = "COM1";

                    _sp.BaudRate = 115200;
                    _sp.Parity = Parity.None;
                    _sp.StopBits = StopBits.One;
                    _sp.DataBits = 8;
                    _sp.ReadTimeout = 1000;
                    _sp.WriteTimeout = 120;
                                        
                }
                return _sp;
            }
        }

        #endregion

        public override string DisplayName
        {
            get
            {
                return "SerialPort";
            }
        }

        public override void Conn()
        {
            _sp.Open();
        }

        public override void DisConn()
        {
            if (_sp.IsOpen)
            {
                _sp.Close();
                _sp.Dispose();
            }
        }

        public override bool IsConn
        {
            get { return _sp.IsOpen; }
        }

        public override int SendTimeOut
        {
            get
            {
                return _sp.WriteTimeout;
            }
            set
            {
                _sp.WriteTimeout = value;
            }
        }

        public override int ReceiveTimeOut
        {
            get
            {
                return _sp.ReadTimeout;
            }
            set
            {
                _sp.ReadTimeout = value;
            }
        }

        public override void Receive(out byte[] buffer)
        {
            buffer = null;
            List<byte> _lstByte = new List<byte>();
            byte byteRead;
            byte byteNext;

            //等待帧头 FFFF
            bool _shouldStop = false;
            while (!_shouldStop)
            {
                if ((byteRead = Convert.ToByte(_sp.ReadByte())) == 0xFF)
                {
                    if ((byteRead = Convert.ToByte(_sp.ReadByte())) == 0xFF)
                    {
                        _shouldStop = true;
                    }
                }
            }

            while ((byteRead = Convert.ToByte(_sp.ReadByte())) != 0xFF)
            {
                if (byteRead != 0xFE)
                {
                    _lstByte.Add(byteRead);
                }
                else
                {
                    byteNext = Convert.ToByte(_sp.ReadByte());
                    if (byteNext != 0x00 && byteNext != 0x01)
                    {
                        buffer = null;
                        return;
                    }
                    else
                    {
                        _lstByte.Add((byte)(byteRead + byteNext));
                    }
                }
            }

            if (_lstByte.Count <= 0)
            {
                buffer = null;
                return;
            }

            buffer = _lstByte.ToArray();
        }


        public override void Send(byte[] sendBuff, int index, int count)
        {
            byte[] dest;
            base.PcRsuFrameDown(sendBuff, index, count, out dest);
            if (dest == null) return;
            _sp.Write(dest, 0, dest.Length);
        }

        public override void ClearReceiveBuffer()
        {
            _sp.DiscardInBuffer();
            
        }
    }
}
