using System.Collections.Generic;
using osu.Framework.Logging;
using Symcol.Core.Networking.Packets;
using Symcol.osu.Mods.Multi.Networking.Packets;
using Symcol.Server.Networking;

namespace Symcol.Server.Mod.osu.Networking
{
    public class OsuNetworkingClientHandler : ServerNetworkingClientHandler
    {
        protected override string Gamekey => "osu";

        protected List<MatchListPacket.MatchInfo> Matches = new List<MatchListPacket.MatchInfo>();

        public OsuNetworkingClientHandler()
        {
            Matches = new List<MatchListPacket.MatchInfo>
            {
                new MatchListPacket.MatchInfo(),
                new MatchListPacket.MatchInfo
                {
                    Name = "First vitaru lobby!",
                    Username = "Shawdooow",
                    UserID = 7726082,
                    UserCountry = "US",
                    BeatmapTitle = "Lost Emotion",
                    BeatmapArtist = "Masayoshi Minoshima feat.nomico",
                    BeatmapStars = 4.5d,
                    RulesetID = 4
                },
            };
        }

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
                    SignPacket(matchList);
                    matchList.MatchInfoList = Matches;
                    GetNetworkingClient(GetClientInfo(getMatch)).SendPacket(matchList);
                    break;
            }
        }
    }
}
