using System.Net;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Plugin
{
    public abstract class ClientConnectedHandler : GenericPlugin
    {
        /// <summary>
        /// Handle client connected callback. Useful for IPblocking.
        /// </summary>
        /// <param name="Client"></param>
        /// <returns>Return true to allow the connection, return false to deny it.</returns>
        public abstract bool OnConnect(Client Client, IPEndPoint IP);
        public abstract bool Enabled { get; set; }
    }
}
