using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using socks5_minimal;
using socks5_minimal.Socks;
using socks5_minimal.TCP;

namespace Socks5_Minimal_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Socks5Server f = new Socks5Server(IPAddress.Any, 10003);
            f.Authentication = true;
            f.OnAuthentication += f_OnAuthentication;
            f.Start();
        }

        static LoginStatus f_OnAuthentication(object sender, SocksAuthenticationEventArgs e)
        {
            if(e.User.Username == "Thr" && e.User.Password == "yoloswag")
                return LoginStatus.Correct;
            return LoginStatus.Denied;
        }
    }
}
