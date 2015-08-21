using System;
using Socona.Fiveocks.Socks;

namespace Socona.Fiveocks.Socks5Client.Events
{
    public class Socks5ClientArgs : EventArgs
    {
        public Socks5ClientArgs(Socks5Client p, SocksError x)
        {
            sock = p;
            status = x;
        }
        private Socks5Client sock = null;
        private SocksError status = SocksError.Failure;
        public SocksError Status { get { return status; } }
        public Socks5Client Client { get { return sock; } }
    }
}
