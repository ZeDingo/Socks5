using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Socona.Fiveocks.TCP
{
    public class Client : IDisposable
    {
        public event EventHandler<ClientEventArgs> ClientDisconnecting;

        public event EventHandler<DataEventArgs> onDataReceived = delegate { };
        public event EventHandler<DataEventArgs> onDataSent = delegate { };

        public Socket Sock { get; set; }
        // private byte[] buffer;
        private int packetSize = 4096;
        public bool Receiving = false;

        public Client(Socket sock, int PacketSize)
        {
            //start the data exchange.
            Sock = sock;
            ClientDisconnecting = delegate { };
            //  buffer = BufferManager.DefaultManager.CheckOut();
            packetSize = PacketSize;
        }

        private bool SocketConnected(Socket s)
        {
            if (!s.Connected) return false;
            bool part1 = s.Poll(10000, SelectMode.SelectError);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        public int Receive(byte[] data, int offset, int count)
        {
            try
            {

                int received = this.Sock.Receive(data, offset, count, SocketFlags.None);
                if (received <= 0)
                {
#if DEBUG
                    Console.WriteLine(string.Format("DCing: recvd={0},Err={1}", received, Sock.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error)));
#endif
                    this.Disconnect();
                    return -1;
                }
                DataEventArgs dargs = new DataEventArgs(data, received);
                //this.onDataReceived(this, dargs);
                return received;
            }
            catch (SocketException ex)
            {
                Console.WriteLine(">>>> Error In Receive! DCing! <<<<");
                this.Disconnect();
                return -1;
            }
        }



        public async Task ReceiveAsyncNew()
        {
            Receiving = true;
            // Reusable SocketAsyncEventArgs and awaitable wrapper 
            var args = new SocketAsyncEventArgs();
            var buffer = BufferManager.DefaultManager.CheckOut();
            args.SetBuffer(buffer, 0, buffer.Length);
            var awaitable = new SocketAwaitable(args);

            // Do processing, continually receiving from the socket 

            while (Sock!=null)
            {
                try
                {
                    if (!Sock.Connected)
                    {
                        break;
                    }
                    await Sock.ReceiveAsync(awaitable);
                    int bytesRead = args.BytesTransferred;
                    if (bytesRead <= 0)
                        break;

                    DataEventArgs data = new DataEventArgs(buffer, bytesRead);
                    this.onDataReceived(this, data);
                }
                catch (SocketException ex)
                {
                    break;
                }
                //catch (NullReferenceException ex)
                //{
                //    break;
                //}
            }
            Receiving = false;
            BufferManager.DefaultManager.CheckIn(buffer);
            Disconnect();
        }
        public void Disconnect()
        {
            if (!this.disposed)
            {
                ClientDisconnecting(this, new ClientEventArgs(this));
                if (Sock != null)
                {
                    try
                    {
#if DEBUG
                        Console.WriteLine("DC'ing... @" + (Sock.Connected ? Sock.RemoteEndPoint.ToString() : "DC'd"));
#endif
                        this.Sock.Shutdown(SocketShutdown.Both);

                    }
                    catch (SocketException sex)
                    {
#if DEBUG
                        Console.WriteLine("Disconnecting... @" + sex.SocketErrorCode);
#endif
                    }

                    this.Sock.Close();
                }
                this.Dispose();
            }
        }

        private void DataSent(IAsyncResult res)
        {
            try
            {
                int sent = ((Socket)res.AsyncState).EndSend(res);

                if (sent < 0)
                {
                    this.Sock.Shutdown(SocketShutdown.Send);
                    Disconnect();
                    return;
                }
#if DEBUG
                Console.WriteLine("Data Sent: " + sent / 1024.0 + "KB");
#endif
                DataEventArgs data = new DataEventArgs(new byte[0] { }, sent);
                this.onDataSent(this, data);
            }
            catch { this.Disconnect(); }
        }

        public bool Send(byte[] buff)
        {
            return Send(buff, 0, buff.Length);
        }

        public void SendAsync(byte[] buff, int offset, int count)
        {
            try
            {
                if (this.Sock != null && this.Sock.Connected)
                {
                    this.Sock.BeginSend(buff, offset, count, SocketFlags.None, new AsyncCallback(DataSent), this.Sock);
                }
            }
            catch
            {
                Console.WriteLine(">>>> Error In SendAsync! DCing! <<<<");
                this.Disconnect();
            }
        }

        public bool Send(byte[] buff, int offset, int count)
        {
            try
            {
                if (this.Sock != null)
                {
                    if (this.Sock.Send(buff, offset, count, SocketFlags.None) <= 0)
                    {
                        Console.WriteLine("Send Dcing");
                        this.Disconnect();
                        return false;
                    }
                    DataEventArgs data = new DataEventArgs(buff, count);
                    this.onDataSent(this, data);
                    return true;
                }
                return false;
            }
            catch
            {
                Console.WriteLine(">>>> Error In Send! DCing! <<<<");
                this.Disconnect();
                return false;
            }
        }
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here. 
                //
                Sock = null;
                //  BufferManager.DefaultManager.CheckIn(buffer);
                //  buffer = null;
                ClientDisconnecting = null;
                onDataReceived = null;
                onDataSent = null;
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
        }
    }
}
