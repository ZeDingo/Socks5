using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using socks5.Plugin;
using socks5.TCP;

namespace Socona.Fiveocks
{
    class FuckGfwPlugin:ConnectSocketOverrideHandler
    {
        private string proxyurl = "127.0.0.1:8087";
        private bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public override socks5.TCP.Client OnConnectOverride(socks5.Socks.SocksRequest sr)
        {
            if (!IsGfwFucked(sr.Address))
                return null;
            var proxysock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            HttpWebRequest httpreq = HttpWebRequest.CreateHttp(sr.Address);
            WebProxy wp = httpreq.Proxy as WebProxy;
            wp.Address=new Uri(proxyurl);
            var response = httpreq.GetResponse(); 
            
            var client = new Client(new Socket(SocketType.Stream, ProtocolType.Tcp), 65535);


            return client;
        }

        public bool IsGfwFucked(string url)
        {
            return false;
        }
    }
}
