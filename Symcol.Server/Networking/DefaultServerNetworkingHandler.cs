using osu.Framework.Logging;
using Symcol.Networking.NetworkingHandlers.Server;
using Symcol.Networking.Packets;

namespace Symcol.Server.Networking
{
    public class DefaultServerNetworkingHandler : ServerNetworkingHandler
    {
        public GameInfo RunningGame;

        protected override void HandlePackets(Packet packet)
        {
            if (!HandlePacket(packet))
                return;

            base.HandlePackets(packet);
        }

        /// <summary>
        /// Should we handle this packet? (is it even for this game?)
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        protected override bool HandlePacket(Packet packet)
        {
            if (GetClient(packet) != null)
                return true;

            if (packet is ConnectPacket c && c.Gamekey == RunningGame.Gamekey)
                return true;

            Logger.Log($"This is not a packet we should handle!", LoggingTarget.Network, LogLevel.Debug);
            return false;
        }
    }
}
