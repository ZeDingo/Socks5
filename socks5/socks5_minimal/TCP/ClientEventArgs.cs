using System;
using socks5_minimal.Socks;

namespace socks5_minimal.TCP
{
    public class ClientEventArgs : EventArgs
    {
        public Client Client { get; private set; }
        public ClientEventArgs(Client client)
        {
            Client = client;
        }
    }
    public class SocksClientEventArgs : EventArgs
    {
        public SocksClient Client { get; private set; }
        public SocksClientEventArgs(SocksClient client)
        {
            Client = client;
        }
    }

    public class SocksAuthenticationEventArgs : EventArgs
    {
        public User User { get; private set; }
        public SocksAuthenticationEventArgs(User loginInfo)
        {
            User = loginInfo;
        }
    }
}
