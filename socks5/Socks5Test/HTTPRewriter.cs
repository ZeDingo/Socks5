using System;
using System.Collections.Generic;
using System.Text;
using Socona.Fiveocks;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;

namespace Socks5Test
{
    class HTTPRewriter : DataHandler
    {
        public override void OnDataReceived(object sender, DataEventArgs e)
        {
            if (e.Buffer.FindString("HTTP/1.") != -1 && e.Buffer.FindString("\r\n") != -1)
            {
                e.Buffer = e.Buffer.ReplaceString("\r\n", "\r\nX-Served-By: Socks5Server\r\n");
                e.Count = e.Count + "X-Served-By: Socks5Server\r\n".Length;
            }
        }

        public override void OnDataSent(object sender, DataEventArgs e)
        {
           
        }

        private bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
    }
}
