using Socona.Fiveocks.Socks;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Plugin
{
    public abstract class ConnectSocketOverrideHandler : GenericPlugin
    {
        public abstract bool Enabled { get; set; }
        /// <summary>
        /// Override the connection, to do whatever you want with it. Client is a wrapper around a socket.
        /// </summary>
        /// <param name="sr">The original request params.</param>
        /// <returns></returns>
        public abstract Client OnConnectOverride(SocksRequest sr);
    }
}
