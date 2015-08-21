using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.Socks;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.SocksServer
{
    public class Socks5Server
    {
        public int Timeout { get; set; }
        public int PacketSize { get; set; }
        public bool LoadPluginsFromDisk { get; set; }

        public TcpServer _server;
        private Thread NetworkStats;

        public List<SocksClient> Clients = new List<SocksClient>();
        public Stats Stats;

        private bool started;

        public event EventHandler OnClientConnected;
        public event EventHandler OnClientDisconnected;

        public event EventHandler OnDebugEvent;

        public Socks5Server(IPAddress ip, int port)
        {
            Timeout = 5000;
            PacketSize = 65535;
            LoadPluginsFromDisk = false;
            Stats = new Stats();
            _server = new TcpServer(ip, port);
            _server.ClientConnected += ServerClientConnected;
        }

        public void Start()
        {
            if (started) return;
            Plugin.PluginLoader.LoadPluginsFromDisk = LoadPluginsFromDisk;
            PluginLoader.LoadPlugins();
            _server.PacketSize = PacketSize;
            _server.Start();
            started = true;
            //start thread.
            // NetworkStats = new Thread(new ThreadStart(delegate()
            //  {
            Task.Factory.StartNew(async () =>
            {
                while (started)
                {
                    if (this.Clients.Contains(null))
                        this.Clients.Remove(null);
                    Stats.ResetClients(this.Clients.Count);
                    await Task.Delay(1000);
                }
            });
            // }));
            //NetworkStats.Start();
        }

        public void Stop()
        {
            if (!started) return;
            _server.Stop();
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].Dispose();
            }
            Clients.Clear();
            started = false;
        }

        void ServerClientConnected(object sender, ClientEventArgs e)
        {
#if DEBUG
            Console.WriteLine("New Client :");
#endif
            //call plugins related to ClientConnectedHandler.
            foreach (ClientConnectedHandler cch in PluginLoader.LoadPlugin(typeof(ClientConnectedHandler)))
                if (cch.Enabled)
                {
                    try
                    {
                        if (!cch.OnConnect(e.Client, (IPEndPoint)e.Client.Sock.RemoteEndPoint))
                        {
                            e.Client.Disconnect();
                            return;
                        }
                    }
                    catch
                    {
                    }
                }

            SocksClient client = new SocksClient(e.Client);
            e.Client.onDataReceived += Client_onDataReceived;
            e.Client.onDataSent += Client_onDataSent;
            client.onClientDisconnected += client_onClientDisconnected;
            Clients.Add(client);
            try
            {
                client.Start(this.PacketSize, this.Timeout);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
                Clients.Remove(client);
                client.Dispose();
                client = null;
            }

        }

        void client_onClientDisconnected(object sender, SocksClientEventArgs e)
        {
            e.Client.onClientDisconnected -= client_onClientDisconnected;
            e.Client.Client.onDataReceived -= Client_onDataReceived;
            e.Client.Client.onDataSent -= Client_onDataSent;
            this.Clients.Remove(e.Client);
        }

        void Client_onDataSent(object sender, DataEventArgs e)
        {
            this.Stats.AddBytes(e.Count, ByteType.Sent);
            this.Stats.AddPacket(PacketType.Sent);

        }

        void Client_onDataReceived(object sender, DataEventArgs e)
        {
            this.Stats.AddBytes(e.Count, ByteType.Received);
            this.Stats.AddPacket(PacketType.Received);
        }
    }
}
