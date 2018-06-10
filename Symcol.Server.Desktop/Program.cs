using osu.Framework;
using osu.Framework.Platform;

namespace Symcol.Server.Desktop
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (GameHost host = Host.GetSuitableHost("SymcolServer"))
            {
                host.Run(new SymcolServer());
            }
        }
    }
}
