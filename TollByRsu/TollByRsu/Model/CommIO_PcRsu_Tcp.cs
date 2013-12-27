using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model
{
    public class CommIO_PcRsu_Tcp : CommIO_PcRsu
    {
        #region constructor

        public CommIO_PcRsu_Tcp()
        {
            _s = new Socket(SocketType.Stream, ProtocolType.Tcp);

        }

        #endregion

        #region dada

        private Socket _s;
        public Socket sClient { get { return _s; } }

        //RSU地址  IP地址+端口号；默认为192.168.9.159：21003

        private IPAddress _TheIpAddress = IPAddress.Parse("192.168.9.159");
        public IPAddress TheIpAddress
        {
            get { return _TheIpAddress; }
            set { if (value == _TheIpAddress) return; _TheIpAddress = value; }
        }

        private int _ThePort = 21003;
        public int ThePort
        {
            get { return _ThePort; }
            set { if (value == _ThePort) return; _ThePort = value; }
        }


        #endregion

        public override string DisplayName
        {
            get
            {
                return "TCP/IP";
            }
        }

        public override void Conn()
        {
            if (_s.Connected) { throw new Exception("already connected, please disconn first"); }

            _s = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _s.Connect(TheIpAddress, ThePort);
        }

        public override void DisConn()
        {
            if (_s.Connected)
            {
                //_s.Disconnect(false);
                //_s.Dispose();
                _s.Close();
            }
        }

        public override bool IsConn
        {
            get
            {
                return sClient.Connected;
            }
        }

        public override int SendTimeOut
        {
            get
            {
                return _s.SendTimeout;
            }
            set
            {
                _s.SendTimeout = value;
            }
        }

        public override int ReceiveTimeOut
        {
            get
            {
                return _s.ReceiveTimeout;
            }
            set
            {
                _s.ReceiveTimeout = value;
            }
        }

        private byte[] _recvBuffer = new byte[1024];
        public override void Receive(out byte[] buffer)
        {
            int n = _s.Receive(_recvBuffer);

            base.PcRsuFrameUp(_recvBuffer, 0, n, out buffer);
        }

        public override void Send(byte[] sendBuff, int index, int count)
        {
            byte[] dest;
            base.PcRsuFrameDown(sendBuff, index, count, out dest);
            if (dest == null) return;

            _s.Send(dest);
        }

        public override void ClearReceiveBuffer()
        {
            return;
        }
    }
}
