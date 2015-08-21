using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Socona.Fiveocks.ExamplePlugins;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.Socks;
using Socona.Fiveocks.Socks5Client.Events;
using Socona.Fiveocks.SocksServer;
using Socona.Fiveocks.TCP;
using Timer = System.Timers.Timer;

namespace Socona.Fiveocks
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool testing = false;
        private int port;
        private Socks5Server x;
        private Timer timer;
        private TextWriter normalOutput;

        private MemoryStream logStream;
        private StreamWriter logWriter;
        private StreamReader logReader;


        private int timetickcnt;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logStream = new MemoryStream();
            logWriter = new StreamWriter(logStream);
            logReader = new StreamReader(logStream);
            port = 80;
            txtIpAddr.Text = IPAddress.Any.ToString() + ":" + port.ToString();
            x = new Socks5Server(IPAddress.Any, port);
            x.Start();
            TestServer();

            normalOutput = Console.Out;

            Console.SetOut(logWriter);


            timer = new Timer(500);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timetickcnt++;
            if (timetickcnt > 480)
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtLog.Clear();
                });

                timetickcnt = 0;
                TestServer();

            }
            if (timetickcnt % 2 == 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtAvailableBuffer.Text = BufferManager.DefaultManager.AvailableBuffers.ToString();
                    if (x != null)
                    {
                        txtDownSpeed.Text = x.Stats.BytesSentPerSec;
                        txtUpSpeed.Text = x.Stats.BytesReceivedPerSec;

                        txtSumRecv.Text = x.Stats.TotalReceived;

                        txtSumSend.Text = x.Stats.TotoalSent;
                        txtClients.Text = x.Stats.TotalClients.ToString();
                    }
                });
            }
            string str = "";
            lock (logStream)
            {
                logStream.Seek(0, SeekOrigin.Begin);
                str = logReader.ReadToEnd();
                logStream.SetLength(0);
            }
            this.Dispatcher.Invoke(() =>
            {
                txtLog.Text += str;
                txtLog.ScrollToEnd();
            });
        }

        void TestServer()
        {

            if (testing == false)
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtStatus.Text = "TEST";
                    txtStatus.Background = new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0x00));
                });

                testing = true;
                try
                {

                    Socks5Client.Socks5Client p = new Socks5Client.Socks5Client("localhost", 80, "www.baidu.com", 80, "lemur", "bison");
                    // p.OnConnected += p_OnConnected;
                    if (p.Connect())
                    {
                        DoLog("====       TEST OK       ====");
                        this.Dispatcher.Invoke(() =>
                        {
                            txtStatus.Text = "OK";
                            txtStatus.Background = new SolidColorBrush(Color.FromRgb(0x00, 0xff, 0x00));
                        });
                        p.Disconnect();
                    }
                    else
                    {
                        DoLog("====        CHK NET        ====");
                        this.Dispatcher.Invoke(() =>
                        {
                            txtStatus.Text = "LOCAL";
                            txtStatus.Background = new SolidColorBrush(Color.FromRgb(0xff, 0x88, 0x00));
                        });
                    }


                    //p.Send(new byte[] {0x11, 0x21});

                }
                catch (Exception ex)
                {
                    DoLog("####     TEST FAIL    ####");
                    this.Dispatcher.Invoke(() =>
                    {
                        txtStatus.Text = "OFF";
                        txtStatus.Background = new SolidColorBrush(Color.FromRgb(0xff, 0x00, 0x00));
                    });
                    btnResetSrv_Click(null, null);
                }
                finally
                {
                    testing = false;
                }
            }
        }
        private void DoLog(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                txtLog.Text += "> " + text + "\r\n";
                txtLog.ScrollToEnd();
            });
        }


        static byte[] m;
        void p_OnConnected(object sender, Socks5ClientArgs e)
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
                DoLog(string.Format("Failed to connect: {0}.", e.Status.ToString()));
            }
        }

        void Client_OnDisconnected(object sender, Socks5ClientArgs e)
        {
            //disconnected.
            DoLog("Disconnected");
        }

        void Client_OnDataReceived(object sender, Socks5ClientDataArgs e)
        {
            DoLog(string.Format("Received {0} bytes from server.", e.Count));
            e.Client.Send(e.Buffer, 0, e.Count);
            e.Client.ReceiveAsync();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (normalOutput != null)
            {
                Console.SetOut(normalOutput);
            }
            if (timer != null)
            {
                timer.Stop();
            }
            if (x != null)
            {
                x.Stop();
            }
        }

        private void txtLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtLog.Text.Count(t => t == '\n') > 400)
            {

            }
        }

        private void btnResetSrv_Click(object sender, RoutedEventArgs e)
        {

            var thread = new Thread(() =>
            {
                if (x != null)
                {

                    DoLog("SERVER STOPPING!");
                    try
                    {
                        x.Stop();
                    }
                    catch (Exception ex)
                    {
                        DoLog(ex.Message);
                    }
                    x = null;

                    DoLog("\tSERVER STOPPED!");
                }
                DoLog("SERVER RESTARTING!");
                x = new Socks5Server(IPAddress.Any, port);
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
                Socks5Client.Socks5Client p = new Socks5Client.Socks5Client("localhost", 80, "127.0.0.1", 80, "lemur", "bison");
                p.OnConnected += p_OnConnected;
                p.ConnectAsync();
                //while (true)
                //{
                // //   Console.Clear();
                //    Console.Write("Total Clients: \t{0}\nTotal Recvd: \t{1:0.00##}MB\nTotal Sent: \t{2:0.00##}MB\n", x.Stats.TotalClients, ((x.Stats.NetworkReceived / 1024f) / 1024f), ((x.Stats.NetworkSent / 1024f) / 1024f));
                //    Console.Write("Receiving/sec: \t{0}\nSending/sec: \t{1}", x.Stats.BytesReceivedPerSec, x.Stats.BytesSentPerSec);
                //    Thread.Sleep(1000);
                //}
                DoLog("\tSERVER RESTARTED!");
            });
            thread.Start();

        }

        private void btnStopSrv_Click(object sender, RoutedEventArgs e)
        {
            if (x != null)
            {
                var thread = new Thread(() =>
                {
                    DoLog("SERVER STOPPING!");
                    x.Stop();
                    x = null;
                    DoLog("\tSERVER STOPPED!");
                });
                thread.Start();

            }
        }
    }
}
