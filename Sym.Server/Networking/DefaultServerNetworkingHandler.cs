using osu.Framework.Logging;
using Sym.Networking.NetworkingHandlers.Server;
using Sym.Networking.Packets;

namespace Sym.Server.Networking
{
    public class DefaultServerNetworkingHandler : ServerNetworkingHandler
    {
        public GameInfo RunningGame;

        protected override void HandlePackets(PacketInfo info)
        {
            if (!HandlePacket(info.Packet))
                return;

            base.HandlePackets(info);
        }

        /// <summary>
        /// Should we handle this packet? (is it even for this game?)
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        protected override bool HandlePacket(Packet packet)
        {
            if (GetLastClient() != null)
                return true;

            if (packet is ConnectPacket c && c.Gamekey == RunningGame.Gamekey)
                return true;

            Logger.Log($"This is not a packet we should handle!", LoggingTarget.Network, LogLevel.Debug);
            return false;
        }
    }
}
