using Symcol.Networking.NetworkingHandlers.Server;
using Symcol.Server.Networking;

namespace Symcol.Server.Mods
{
    public abstract class Modset
    {
        public virtual ServerNetworkingHandler GetServerNetworkingClientHandler() => new DefaultServerNetworkingHandler();
    }
}
