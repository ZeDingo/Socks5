using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Socks
{
    class SocksSpecialTunnel : IDisposable
    {
        public SocksRequest Req;
        public SocksRequest ModifiedReq;

        public SocksClient Client;
        public Client RemoteClient;

        private List<DataHandler> Plugins = new List<DataHandler>();

        private int Timeout = 10000;
        private int PacketSize = 4096;
        private SocksEncryption se;
        public event EventHandler TunnelDisposing;
        private bool isDisposing;
        public SocksSpecialTunnel(SocksClient p, SocksEncryption ph, SocksRequest req, SocksRequest req1, int packetSize, int timeout)
        {
            RemoteClient = new Client(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), PacketSize);
            Client = p;
            Req = req;
            ModifiedReq = req1;
            PacketSize = packetSize;
            Timeout = timeout;
            se = ph;
            isDisposing = false;
        }

        public void Start()
        {
            if (ModifiedReq.Address == null || ModifiedReq.Port <= -1) { Client.Client.Disconnect(); return; }
#if DEBUG
            Console.WriteLine("{0}:{1}", ModifiedReq.Address, ModifiedReq.Port);
#endif
            foreach (ConnectSocketOverrideHandler conn in PluginLoader.LoadPlugin(typeof(ConnectSocketOverrideHandler)))
                if (conn.Enabled)
                {
                    Client pm = conn.OnConnectOverride(ModifiedReq);
                    if (pm != null)
                    {
                        //check if it's connected.
                        if (pm.Sock.Connected)
                        {
                            RemoteClient = pm;
                            //send request right here.
                            byte[] shit = Req.GetData(true);
                            shit[1] = 0x00;
                            //process packet.
                            byte[] output = se.ProcessOutputData(shit, 0, shit.Length);
                            //gucci let's go.
                            Client.Client.Send(output);
                            ConnectHandler(null);
                            return;
                        }
                    }
                }
            var socketArgs = new SocketAsyncEventArgs { RemoteEndPoint = new IPEndPoint(ModifiedReq.IP, ModifiedReq.Port) };
            socketArgs.Completed += socketArgs_Completed;
            RemoteClient.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (!RemoteClient.Sock.ConnectAsync(socketArgs))
                ConnectHandler(socketArgs);
        }

        void socketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            byte[] request = Req.GetData(true); // Client.Client.Send(Req.GetData());
            if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine("Error while connecting: {0}", e.SocketError.ToString());
                request[1] = (byte)SocksError.Unreachable;
            }
            else
            {
                request[1] = 0x00;
            }

            byte[] encreq = se.ProcessOutputData(request, 0, request.Length);
            Client.Client.Send(encreq);

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    //connected;
                    ConnectHandler(e);
                    break;
            }
        }

        private void ConnectHandler(SocketAsyncEventArgs e)
        {
            //start receiving from both endpoints.
            try
            {
                //all plugins get the event thrown.
                foreach (DataHandler data in PluginLoader.LoadPlugin(typeof(DataHandler)))
                    Plugins.Push(data);
                Client.Client.onDataReceived += Client_onDataReceived;
                RemoteClient.onDataReceived += RemoteClient_onDataReceived;
                RemoteClient.ClientDisconnecting += RemoteClientClientDisconnecting;
                Client.Client.ClientDisconnecting += ClientClientDisconnecting;
                Client.Client.ReceiveAsyncNew();
                RemoteClient.ReceiveAsyncNew();
            }
            catch
            {
            }
        }
        bool disconnected = false;
        //  private bool remotedcd = false;
        void ClientClientDisconnecting(object sender, ClientEventArgs e)
        {
            if (disconnected) return;
            disconnected = true;
            RemoteClient.Disconnect();
            OnTunnelDisposing();
        }

        void RemoteClientClientDisconnecting(object sender, ClientEventArgs e)
        {
#if DEBUG
            Console.WriteLine("Remote DC'd");
#endif
            if (disconnected) return;
            disconnected = true;
            Client.Client.Disconnect();
            OnTunnelDisposing();
        }

        void RemoteClient_onDataReceived(object sender, DataEventArgs e)
        {
            e.Request = this.ModifiedReq;
            try
            {
                foreach (DataHandler f in Plugins)
                    if (f.Enabled)
                        f.OnDataReceived(this, e);
                //craft headers & shit.
                byte[] outputdata = se.ProcessOutputData(e.Buffer, e.Offset, e.Count);
                //send outputdata's length firs.t
                Client.Client.Send(BitConverter.GetBytes(outputdata.Length));
                e.Buffer = outputdata;
                e.Offset = 0;
                e.Count = outputdata.Length;
                //ok now send data.
                Client.Client.Send(e.Buffer, e.Offset, e.Count);
                //  if (!RemoteClient.Receiving)
                //       RemoteClient.ReceiveAsyncNew();
                //   if (!Client.Client.Receiving)
                //        Client.Client.ReceiveAsyncNew();

            }
            catch
            {
                Client.Client.Disconnect();
                RemoteClient.Disconnect();
                OnTunnelDisposing();
            }
        }

        void Client_onDataReceived(object sender, DataEventArgs e)
        {
            e.Request = this.ModifiedReq;
            //this should be packet header.
            try
            {
                int torecv = BitConverter.ToInt32(e.Buffer, e.Offset);
                byte[] newbuff = new byte[torecv];
                int recv = Client.Client.Receive(newbuff, 0, newbuff.Length);
                if (recv == torecv)
                {
                    //yey
                    //process packet.
                    byte[] output = se.ProcessInputData(newbuff, 0, recv);
                    e.Buffer = output;
                    e.Offset = 0;
                    e.Count = output.Length;
                    //receive full packet.
                    foreach (DataHandler f in Plugins)
                        if (f.Enabled)
                            f.OnDataSent(this, e);
                    RemoteClient.SendAsync(e.Buffer, e.Offset, e.Count);
                    //    if (!Client.Client.Receiving)
                    //        Client.Client.ReceiveAsyncNew();
                    //   if (!RemoteClient.Receiving)
                    //     RemoteClient.ReceiveAsyncNew();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                //disconnect.
                Client.Client.Disconnect();
                RemoteClient.Disconnect();
                OnTunnelDisposing();
            }
        }

        protected void OnTunnelDisposing()
        {
            if (this.TunnelDisposing != null)
            {
                TunnelDisposing(this, new EventArgs());
            }
            Dispose();
        }
        public void Dispose()
        {
            if (isDisposing)
            {
                return;
            }
            isDisposing = true;
            disconnected = true;
            this.Client = null;
            this.RemoteClient = null;
            this.ModifiedReq = null;
            this.Req = null;
        }


    }
}
