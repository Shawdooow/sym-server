using Symcol.Server.Networking;

namespace Symcol.Server.Mods
{
    public abstract class Modset
    {
        public virtual ServerNetworkingClientHandler GetServerNetworkingClientHandler() => new ServerNetworkingClientHandler();
    }
}
