using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Socona.Fiveocks.TCP
{
    public class TcpServer
    {
        private TcpListener p;
        private bool accept = false;
        public int PacketSize { get; set; }

        public event EventHandler<ClientEventArgs> ClientConnected = delegate { };
        public event EventHandler<ClientEventArgs> ClientDisconnecting = delegate { };

        public event EventHandler<DataEventArgs> onDataReceived = delegate { };
        public event EventHandler<DataEventArgs> onDataSent = delegate { };

        public TcpServer(IPAddress ip, int port)
        {
            p = TcpListener.Create(port);

        }

        // private ManualResetEventSlim signal = new ManualResetEventSlim(false);

        private async void AcceptConnections()
        {
            while (accept)
            {
                try
                {
                    Socket x = await p.AcceptSocketAsync();

                    if (x == null)
                    {
                        break;
                    }
                    Client f = new Client(x, PacketSize);
                    f.ClientDisconnecting += ClientDisconnecting;
                    f.onDataReceived += onDataReceived;
                    f.onDataSent += onDataSent;
                    ClientConnected(this, new ClientEventArgs(f));
                }
                catch (ObjectDisposedException ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        public void Start()
        {
            if (!accept)
            {
                accept = true;
                p.Start(1000);
                Task.Factory.StartNew(AcceptConnections);
            }
        }

        public void Stop()
        {
            if (accept)
            {
                accept = false;
                p.Stop();
                p = null;

            }
        }
    }
}
