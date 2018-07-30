using osu.Framework.Logging;
using Symcol.Core.Networking;
using Symcol.Core.Networking.Packets;

namespace Symcol.Server.Networking
{
    public class ServerNetworkingClientHandler : NetworkingClientHandler
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
        protected virtual bool HandlePacket(Packet packet)
        {
            if (GetClientInfo(packet) != null)
                return true;

            if (packet is ConnectPacket c && c.Gamekey == RunningGame.Gamekey)
                return true;

            Logger.Log($"This is not a packet we should handle!", LoggingTarget.Network, LogLevel.Debug);
            return false;
        }
    }
}
