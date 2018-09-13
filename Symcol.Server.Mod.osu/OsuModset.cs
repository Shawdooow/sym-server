using Symcol.Networking.NetworkingHandlers.Server;
using Symcol.Server.Mod.osu.Networking;
using Symcol.Server.Mods;

namespace Symcol.Server.Mod.osu
{
    public sealed class OsuModset : Modset
    {
        public override ServerNetworkingHandler GetServerNetworkingClientHandler() => new OsuServerNetworkingClientHandler();
    }
}
