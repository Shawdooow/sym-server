using Symcol.Core.Networking.Packets;
using Symcol.Server.Networking;

namespace Symcol.Server.Mod.osu.Networking
{
    public class OsuNetworkingClientHandler : ServerNetworkingClientHandler
    {
        protected override void HandlePackets(Packet packet)
        {
            if (!HandlePacket(packet))
                return;

            switch (packet)
            {
                default:
                    base.HandlePackets(packet);
                    break;
            }
        }
    }
}
