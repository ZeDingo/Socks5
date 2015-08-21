﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace socks5.TCP
{
    public class TcpServer
    {
        private TcpListener p;
        private bool accept = false;
        public int PacketSize { get; set; }

        public event EventHandler<ClientEventArgs> onClientConnected = delegate { };
        public event EventHandler<ClientEventArgs> onClientDisconnected = delegate { };

        //public event EventHandler<DataEventArgs> onDataReceived = delegate { };
        //public event EventHandler<DataEventArgs> onDataSent = delegate { };

        public TcpServer(IPAddress ip, int port)
        {
            p = TcpListener.Create(port);
        }

        private ManualResetEventSlim signal = new ManualResetEventSlim(false);

        private void AcceptConnections()
        {
            while (accept)
            {
                try
                {
                    signal.Reset();
                    p.BeginAcceptSocket(new AsyncCallback(AcceptClient), p);
                    signal.Wait();
                }
                catch 
                { //error, most likely server shutdown.
                }
            }
        }

        void AcceptClient(IAsyncResult res)
        {
            try
            {
                TcpListener px = (TcpListener) res.AsyncState;
                Socket x = px.EndAcceptSocket(res);
                signal.Set();
                
                Client f = new Client(x, PacketSize);
                f.onClientDisconnected += onClientDisconnected;
                f.onDataReceived += onDataReceived;
                f.onDataSent += onDataSent;
                onClientConnected(this, new ClientEventArgs(f));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //server stopped or client errored?
            }
            finally
            {
                if (!signal.IsSet)
                {
                    signal.Set();
                }
            }
         }

        public void Start()
        {
            if (!accept)
            {
                accept = true;
                p.Start(10000);               
                new Thread(new ThreadStart(AcceptConnections)).Start();
            }
        }

        public void Stop()
        {
            if (accept)
            {
                accept = false;
                p.Stop();
                signal.Set();
            }
        }
    }
}
