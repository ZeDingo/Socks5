using System;
using System.Threading;

namespace Socona.Fiveocks.TCP
{
    public class BandwidthCounter
    {
        private MiniCounter perSecond;
        public long totalbytes;

        /// <summary>
        ///     Empty constructor, because thats constructive
        /// </summary>
        public BandwidthCounter()
        {
            perSecond = new MiniCounter {LastRead = DateTime.Now, Totalbytes = 0};
        }

        /// <summary>
        ///     Accesses the current transfer rate, returning the text
        /// </summary>
        /// <returns></returns>
        public string GetPerSecond()
        {
            var s = perSecond + "/s";
            perSecond = new MiniCounter {LastRead = DateTime.Now, Totalbytes = 0};
            return s;
        }

        /// <summary>
        ///     Adds bytes to the total transfered
        /// </summary>
        /// <param name="count">Byte count</param>
        public void AddBytes(ulong count)
        {
            long currentTotal = 0;
            long newtotal = 0;
            do
            {
                currentTotal = totalbytes;
                newtotal = currentTotal + (long) count;
            } while (Interlocked.CompareExchange(ref totalbytes, newtotal, currentTotal) != currentTotal);
            // overflow max
            perSecond.AddBytes(count);
        }

        /// <summary>
        ///     Prints out a relevant string for the bits transfered
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var curtotal = totalbytes;

            var pbyte = curtotal/(1024L*1024*1024*1024*1024);
            curtotal %= (1024L*1024*1024*1024*1024);
            var tbyte = curtotal/(1024L*1024*1024*1024);
            curtotal %= (1024L*1024*1024*1024);
            var gbyte = curtotal/(1024*1024*1024);
            curtotal %= (1024*1024*1024);
            var mbyte = curtotal/(1024*1024);
            curtotal %= (1024*1024);
            var kbyte = curtotal/1024;
            var nbyte = curtotal%1024;

            if (pbyte > 0)
            {
                var ret = pbyte + (double) tbyte/1024;
                var s = ret.ToString("#0.000");

                return s + " PB";
            }
            if (tbyte > 0)
            {
                var ret = tbyte + (double) gbyte/1024;

                var s = ret.ToString("#0.000");

                return s + " TB";
            }
            if (gbyte > 0)
            {
                var ret = gbyte + (double) mbyte/1024;
                var s = ret.ToString("#0.000");

                return s + " GB";
            }
            if (mbyte > 0)
            {
                var ret = mbyte + (double) kbyte/1024;

                var s = ret.ToString("#0.000");

                return s + " MB";
            }
            if (kbyte > 0)
            {
                var ret = kbyte + (double) nbyte/1024;

                var s = ret.ToString("#0.000");

                return s + " KB";
            }
            else
            {
                var s = nbyte.ToString("#0.000");

                return s + " B";
            }
        }

        /// <summary>
        ///     Class to manage an adapters current transfer rate
        /// </summary>
        private struct MiniCounter
        {
            private long totalbytes;

            public long Totalbytes
            {
                get { return totalbytes; }
                set { totalbytes = value; }
            }

            public DateTime LastRead { get; set; }

            /// <summary>
            ///     Adds bits(total misnomer because bits per second looks a lot better than bytes per second)
            /// </summary>
            /// <param name="count">The number of bits to add</param>
            public void AddBytes(ulong count)
            {
                long currentTotal = 0;
                long newtotal = 0;
                do
                {
                    currentTotal = Totalbytes;
                    newtotal = currentTotal + (long) count;
                } while (Interlocked.CompareExchange(ref totalbytes, newtotal, currentTotal) != currentTotal);
                // Totalbytes += count;
            }

            /// <summary>
            ///     Returns the bits per second since the last time this function was called
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var curtotal = Totalbytes;

                var pbyte = curtotal/(1024L*1024*1024*1024*1024);
                curtotal %= (1024L*1024*1024*1024*1024);
                var tbyte = curtotal/(1024L*1024*1024*1024);
                curtotal %= (1024L*1024*1024*1024);
                var gbyte = curtotal/(1024*1024*1024);
                curtotal %= (1024*1024*1024);
                var mbyte = curtotal/(1024*1024);
                curtotal %= (1024*1024);
                var kbyte = curtotal/1024;
                var nbyte = curtotal%1024;
                if (pbyte > 0)
                {
                    var ret = pbyte + (double) tbyte/1024;
                    ret = ret/(DateTime.Now - LastRead).TotalSeconds;

                    LastRead = DateTime.Now;
                    var s = ret.ToString("#0.000");

                    return s + " PB";
                }
                if (tbyte > 0)
                {
                    var ret = tbyte + (double) gbyte/1024;
                    ret = ret/(DateTime.Now - LastRead).TotalSeconds;

                    LastRead = DateTime.Now;
                    var s = ret.ToString("#0.00");

                    return s + " TB";
                }
                if (gbyte > 0)
                {
                    var ret = gbyte + (double) mbyte/1024;
                    ret = ret/(DateTime.Now - LastRead).TotalSeconds;

                    LastRead = DateTime.Now;
                    var s = ret.ToString("#0.00");

                    return s + " GB";
                }
                if (mbyte > 0)
                {
                    var ret = mbyte + (double) kbyte/1024;
                    ret = ret/(DateTime.Now - LastRead).TotalSeconds;

                    LastRead = DateTime.Now;
                    var s = ret.ToString("#0.00");

                    return s + " MB";
                }
                if (kbyte > 0)
                {
                    var ret = kbyte + (double) nbyte/1024;
                    ret = ret/(DateTime.Now - LastRead).TotalSeconds;
                    LastRead = DateTime.Now;
                    var s = ret.ToString("#0.00");

                    return s + " KB";
                }
                else
                {
                    double ret = nbyte;
                    ret = ret/(DateTime.Now - LastRead).TotalSeconds;
                    LastRead = DateTime.Now;
                    var s = ret.ToString("#0.00");

                    return s + " B";
                }
            }
        }
    }
}