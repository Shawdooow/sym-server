using System.Collections.Generic;
using osu.Framework.Logging;
using Symcol.Core.Networking;
using Symcol.Core.Networking.Packets;

namespace Symcol.Server.Networking
{
    public class ServerNetworkingClientHandler : NetworkingClientHandler
    {
        public readonly List<GameInfo> RunningGames = new List<GameInfo>();

        protected override void HandlePackets(Packet packet)
        {
            base.HandlePackets(packet);

            switch (packet)
            {
                case ConnectPacket connectPacket:
                    GameInfo game = GetGameInfo(connectPacket.GameID);

                    if (game == null)
                    {
                        Logger.Log("A client from a game that we are not running is trying to connect!", LoggingTarget.Network);
                        break;
                    }

                    game.GameClients.Add(GetConnectingClientInfo(connectPacket));
                    break;
                case DisconnectPacket disconnectPacket:
                    break;
            }
        }

        protected GameInfo GetGameInfo(string gameID)
        {
            foreach (GameInfo game in RunningGames)
                if (game.GameID == gameID)
                    return game;
            return null;
        }
    }
}
