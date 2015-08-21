using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace socks5
{
    public class BandwidthCounter
    {
        /// <summary>
        /// Class to manage an adapters current transfer rate
        /// </summary>
        class MiniCounter
        {
            
            public ulong bytes = 0;
            public ulong kbytes = 0;
            public ulong mbytes = 0;
            public ulong gbytes = 0;
            public ulong tbytes = 0;
            public ulong pbytes = 0;
            DateTime lastRead = DateTime.Now;

            /// <summary>
            /// Adds bits(total misnomer because bits per second looks a lot better than bytes per second)
            /// </summary>
            /// <param name="count">The number of bits to add</param>
            public void AddBytes(ulong count)
            {
                bytes += count;
                while (bytes > 1024)
                {
                    kbytes++;
                    bytes -= 1024;
                }
                while (kbytes > 1024)
                {
                    mbytes++;
                    kbytes -= 1024;
                }
                while (mbytes > 1024)
                {
                    gbytes++;
                    mbytes -= 1024;
                }
                while (gbytes > 1024)
                {
                    tbytes++;
                    gbytes -= 1024;
                }
                while (tbytes > 1024)
                {
                    pbytes++;
                    tbytes -= 1024;
                }
            }

            /// <summary>
            /// Returns the bits per second since the last time this function was called
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (pbytes > 0)
                {
                    double ret = (double)pbytes + ((double)((double)tbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString("#0.000");

                    return s + " PB";
                }
                else if (tbytes > 0)
                {
                    double ret = (double)tbytes + ((double)((double)gbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString("#0.00");

                    return s + " TB";
                }
                else if (gbytes > 0)
                {
                    double ret = (double)gbytes + ((double)((double)mbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString("#0.00");

                    return s + " GB";
                }
                else if (mbytes > 0)
                {
                    double ret = (double)mbytes + ((double)((double)kbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString("#0.00");

                    return s + " MB";
                }
                else if (kbytes > 0)
                {
                    double ret = (double)kbytes + ((double)((double)bytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString("#0.00");

                    return s + " KB";
                }
                else
                {
                    double ret = bytes;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString("#0.00");

                    return s + " B";
                }
            }
        }
        public BigInteger totalbytes;
        
        MiniCounter perSecond = new MiniCounter();

        /// <summary>
        /// Empty constructor, because thats constructive
        /// </summary>
        public BandwidthCounter()
        {

        }

        /// <summary>
        /// Accesses the current transfer rate, returning the text
        /// </summary>
        /// <returns></returns>
        public string GetPerSecond()
        {
            string s = perSecond.ToString() + "/s";
            perSecond = new MiniCounter();
            return s;
        }

        /// <summary>
        /// Adds bytes to the total transfered
        /// </summary>
        /// <param name="count">Byte count</param>
        public void AddBytes(ulong count)
        {
            
            totalbytes += count;
            // overflow max
            perSecond.AddBytes(count);
        }

        /// <summary>
        /// Prints out a relevant string for the bits transfered
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var curtotal = totalbytes;

            var pbyte = curtotal / (1024L * 1024 * 1024 * 1024 * 1024);
            curtotal %= (1024L * 1024 * 1024 * 1024 * 1024);
            var tbyte = curtotal / (1024L * 1024 * 1024 * 1024);
            curtotal %= (1024L * 1024 * 1024 * 1024);
            var gbyte = curtotal / (1024 * 1024 * 1024);
            curtotal %= (1024 * 1024 * 1024);
            var mbyte = curtotal / (1024 * 1024);
            curtotal %= (1024 * 1024);
            var kbyte = curtotal / 1024;
            var nbyte = curtotal % 1024;

            if (pbyte > 0)
            {
                double ret = (double)pbyte + ((double)((double)tbyte / 1024));
                string s = ret.ToString("#0.000");

                return s + " PB";
            }
            else if (tbyte > 0)
            {
                double ret = (double)tbyte + ((double)((double)gbyte / 1024));
                string s = ret.ToString("#0.000");

                return s + " TB";
            }
            else if (gbyte > 0)
            {
                double ret = (double)gbyte + ((double)((double)mbyte / 1024));
                string s = ret.ToString("#0.000");

                return s + " GB";
            }
            else if (mbyte > 0)
            {
                double ret = (double)mbyte + ((double)((double)kbyte / 1024));
                string s = ret.ToString("#0.000");

                return s + " MB";
            }
            else if (kbyte > 0)
            {
                double ret = (double)kbyte + ((double)((double)byte / 1024));
                string s = ret.ToString("#0.000");

                return s + " KB";
            }
            else
            {
                string s = nbyte.ToString("#0.000");

                return s + " B";
            }
        }
    }
}
