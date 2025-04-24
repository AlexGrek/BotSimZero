using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI.Terminal
{
    public class Repository
    {
        private Dictionary<string, Func<dynamic, ITerminalApp>> _apps = new Dictionary<string, Func<dynamic, ITerminalApp>>();
        public Repository()
        {
            _apps.Add("HelloWorld", (options) => new HelloWorldTerminalApp());
            _apps.Add("Random", (options) => new RandomTerminalApp());
        }

        public ITerminalApp CreateApp(string appName, dynamic options)
        {
            if (_apps.ContainsKey(appName))
            {
                return _apps[appName](options);
            }
            else
            {
                throw new Exception($"App {appName} not found in repository.");
            }
        }
    }
}
