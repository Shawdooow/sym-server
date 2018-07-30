using System.Collections.Generic;
using osu.Framework.Logging;
using Symcol.Core.Networking.Packets;
using Symcol.osu.Mods.Multi.Networking.Packets;
using Symcol.Server.Networking;

namespace Symcol.Server.Mod.osu.Networking
{
    public class OsuServerNetworkingClientHandler : ServerNetworkingClientHandler
    {
        protected override string Gamekey => "osu";

        protected readonly List<MatchListPacket.MatchInfo> Matches = new List<MatchListPacket.MatchInfo>();

        protected override void HandlePackets(Packet packet)
        {
            Logger.Log($"Recieved a Packet from {packet.Address}", LoggingTarget.Network, LogLevel.Debug);

            if (!HandlePacket(packet))
                return;

            switch (packet)
            {
                default:
                    base.HandlePackets(packet);
                    break;
                case GetMatchListPacket getMatch:
                    MatchListPacket matchList = new MatchListPacket();
                    matchList.MatchInfoList = Matches;
                    SendToClient(matchList, getMatch);
                    break;
                case CreateMatchPacket createMatch:
                    Matches.Add(createMatch.MatchInfo);
                    SendToClient(new MatchCreatedPacket{ MatchInfo = createMatch.MatchInfo }, createMatch);
                    break;
                //case DeleteMatchPacket deleteMatch:
            }
        }
    }
}
