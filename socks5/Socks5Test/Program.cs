﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using Socona.Fiveocks.ExamplePlugins;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.Socks;
using Socona.Fiveocks.Socks5Client;
using Socona.Fiveocks.Socks5Client.Events;
using Socona.Fiveocks.SocksServer;

namespace Socks5Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Socks5Server x = new Socks5Server(IPAddress.Any, 10084);
            x.Start();
            PluginLoader.ChangePluginStatus(false, typeof(DataHandlerExample));
            //enable plugin.
            foreach (object pl in PluginLoader.GetPlugins)
            {
                //if (pl.GetType() == typeof(LoginHandlerExample))
                //{
                //    //enable it.
                //    PluginLoader.ChangePluginStatus(true, pl.GetType());
                //    Console.WriteLine("Enabled {0}.", pl.GetType().ToString());
                //}
            }
            //Start showing network stats.
            Socks5Client p = new Socks5Client("localhost", 10084, "127.0.0.1", 10084, "yolo", "swag");
            p.OnConnected += p_OnConnected;
            p.ConnectAsync();
            //while (true)
            //{
            // //   Console.Clear();
            //    Console.Write("Total Clients: \t{0}\nTotal Recvd: \t{1:0.00##}MB\nTotal Sent: \t{2:0.00##}MB\n", x.Stats.TotalClients, ((x.Stats.NetworkReceived / 1024f) / 1024f), ((x.Stats.NetworkSent / 1024f) / 1024f));
            //    Console.Write("Receiving/sec: \t{0}\nSending/sec: \t{1}", x.Stats.BytesReceivedPerSec, x.Stats.BytesSentPerSec);
            //    Thread.Sleep(1000);
            //}
        }
        static byte[] m;
        static void p_OnConnected(object sender, Socks5ClientArgs e)
        {
            if (e.Status == SocksError.Granted)
            {
                e.Client.OnDataReceived += Client_OnDataReceived;
                e.Client.OnDisconnected += Client_OnDisconnected;
                m = Encoding.ASCII.GetBytes("Start Sending Data:\n");
                e.Client.Send(m, 0, m.Length);
                e.Client.ReceiveAsync();
            }
            else
            {
                Console.WriteLine("Failed to connect: {0}.", e.Status.ToString());
            }
        }

        static void Client_OnDisconnected(object sender, Socks5ClientArgs e)
        {
            //disconnected.
            Console.WriteLine("DC'd");
        }

        static void Client_OnDataReceived(object sender, Socks5ClientDataArgs e)
        {
            Console.WriteLine("Received {0} bytes from server.", e.Count);
            e.Client.Send(e.Buffer, 0, e.Count);
            e.Client.ReceiveAsync();
        }
    }
}
