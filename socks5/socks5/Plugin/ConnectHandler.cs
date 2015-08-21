using Socona.Fiveocks.Socks;

namespace Socona.Fiveocks.Plugin
{
    public abstract class ConnectHandler : GenericPlugin
    {
        /// <summary>
        /// Handle request callback.
        /// </summary>
        /// <param name="Request"></param>
        /// <returns>Return true to allow the connection, return false to deny it.</returns>
        public abstract bool OnConnect(SocksRequest Request);
        public abstract bool Enabled { get; set; }
    }
}
