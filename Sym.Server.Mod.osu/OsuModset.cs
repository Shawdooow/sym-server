using osu.Mods.Online.Base;
using Sym.Networking.NetworkingHandlers.Server;
using Sym.Server.Mods;

namespace Sym.Server.Mod.osu
{
    public sealed class OsuModset : Modset
    {
        public override ServerNetworkingHandler GetServerNetworkingClientHandler() => new OsuServerNetworkingHandler();
    }
}
