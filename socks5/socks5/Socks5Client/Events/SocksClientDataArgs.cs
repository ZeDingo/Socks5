using System;

namespace Socona.Fiveocks.Socks5Client.Events
{
    public class Socks5ClientDataArgs : EventArgs
    {
        public Socks5ClientDataArgs(Socks5Client client, byte[] buff, int count, int offset)
        {
            cli = client;
            Buffer = buff;
            Count = count;
            Offset = offset;
        }
        //Data Buffer
        //Socks Client
        private Socks5Client cli = null;
        public Socks5Client Client { get { return cli; } }
        public byte[] Buffer { get; set; }
        public int Count { get; set; }
        public int Offset { get; set; }
    }
}
