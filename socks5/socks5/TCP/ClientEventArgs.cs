using System;
using Socona.Fiveocks.Socks;

namespace Socona.Fiveocks.TCP
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
}
