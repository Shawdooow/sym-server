using Sym.Networking.NetworkingHandlers.Server;
using Sym.Server.Networking;

namespace Sym.Server.Mods
{
    public abstract class Modset
    {
        public virtual ServerNetworkingHandler GetServerNetworkingClientHandler() => new DefaultServerNetworkingHandler();
    }
}
