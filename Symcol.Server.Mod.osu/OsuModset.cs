using Symcol.Server.Mod.osu.Networking;
using Symcol.Server.Mods;
using Symcol.Server.Networking;

namespace Symcol.Server.Mod.osu
{
    public sealed class OsuModset : Modset
    {
        public override ServerNetworkingClientHandler GetServerNetworkingClientHandler() => new OsuServerNetworkingClientHandler();
    }
}
