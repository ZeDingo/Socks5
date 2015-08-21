using Socona.Fiveocks.Socks;

namespace Socona.Fiveocks.Plugin
{
    public enum LoginStatus
    {
        Denied = 0xFF,
        Correct = 0x00
    }
    public abstract class LoginHandler : GenericPlugin
    {
        public abstract LoginStatus HandleLogin(User user);
        public abstract bool Enabled { get; set; }
    }
}
