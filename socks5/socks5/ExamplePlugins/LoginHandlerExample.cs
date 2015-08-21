using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.Socks;

namespace Socona.Fiveocks.ExamplePlugins
{
    public class LoginHandlerExample : LoginHandler
    {
        public override LoginStatus HandleLogin(User user)
        {
            return LoginStatus.Correct;// (user.Username == "thrdev" && user.Password == "testing1234" ? LoginStatus.Correct : LoginStatus.Denied);
        }
        //Username/Password Table? Endless possiblities for the login system.
        private bool enabled = false;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
    }
}
