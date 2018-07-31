using System.Collections.Generic;
using osu.Framework.Logging;
using Symcol.Core.Networking.Packets;
using Symcol.osu.Mods.Multi.Networking;
using Symcol.osu.Mods.Multi.Networking.Packets.Lobby;
using Symcol.osu.Mods.Multi.Networking.Packets.Match;
using Symcol.Server.Networking;

namespace Symcol.Server.Mod.osu.Networking
{
    public class OsuServerNetworkingClientHandler : ServerNetworkingClientHandler
    {
        protected override string Gamekey => "osu";

        //TODO: if a match is empty for 1 min delete is automatically
        protected readonly List<MatchListPacket.MatchInfo> Matches = new List<MatchListPacket.MatchInfo>();

        protected override void HandlePackets(Packet packet)
        {
            Logger.Log($"Recieved a Packet from {packet.Address}", LoggingTarget.Network, LogLevel.Debug);

            if (!HandlePacket(packet))
                return;

            MatchListPacket.MatchInfo match = null;

            switch (packet)
            {
                default:
                    base.HandlePackets(packet);
                    break;
                case GetMatchListPacket getMatch:
                    MatchListPacket matchList = new MatchListPacket
                    {
                        MatchInfoList = Matches
                    };
                    SendToClient(matchList, getMatch);
                    break;
                case CreateMatchPacket createMatch:
                    Matches.Add(createMatch.MatchInfo);
                    SendToClient(new MatchCreatedPacket{ MatchInfo = createMatch.MatchInfo }, createMatch);
                    break;
                case JoinMatchPacket joinPacket:
                    if (joinPacket.OsuClientInfo == null)
                        break;

                    foreach (MatchListPacket.MatchInfo m in Matches)
                        if (m.BeatmapTitle == joinPacket.Match.BeatmapTitle &&
                            m.BeatmapArtist == joinPacket.Match.BeatmapArtist &&
                            m.Name == joinPacket.Match.Name &&
                            m.Username == joinPacket.Match.Username)
                            match = m;

                    if (match != null)
                    {
                        //Add them
                        match.Players.Add(joinPacket.OsuClientInfo);

                        //Tell them they have joined
                        SendToClient(new JoinedMatchPacket {Players = match.Players}, joinPacket);

                        //Tell everyone already there someone joined
                        ShareWithMatchClients(match, new PlayerJoinedPacket
                        {
                            //This is so the person joining doesnt get sent this
                            Address = joinPacket.Address,
                            Player = joinPacket.OsuClientInfo
                        });
                    }
                    else
                        Logger.Log("Couldn't find a match matching one in packet!", LoggingTarget.Network, LogLevel.Error);

                    break;
                case SetMapPacket map:
                    match = GetMatch(map.Player);

                    match.BeatmapTitle = map.BeatmapTitle;
                    match.BeatmapArtist = map.BeatmapArtist;

                    ShareWithMatchClients(match, map);
                    break;
                case ChatPacket chat:
                    break;
                case LeavePacket leave:
                    if (GetMatch(leave.Player) != null)
                        foreach (OsuClientInfo player in GetMatch(leave.Player).Players)
                            if (player.UserID == leave.Player.UserID)
                            {
                                GetMatch(leave.Player).Players.Remove(player);
                                break;
                            }

                    Logger.Log("Couldn't find a player to remove who told us they were leaving!", LoggingTarget.Network, LogLevel.Error);
                    break;
                case StartMatchPacket start:
                    break;
            }
        }

        protected void ShareWithMatchClients(MatchListPacket.MatchInfo match, Packet packet)
        {
            foreach (OsuClientInfo player in match.Players)
                GetNetworkingClient(player).SendPacket(packet);
        }

        protected MatchListPacket.MatchInfo GetMatch(OsuClientInfo player)
        {
            foreach (MatchListPacket.MatchInfo m in Matches)
                foreach (OsuClientInfo p in m.Players)
                    if (p.UserID == player.UserID)
                        return m;
            return null;
        }
    }
}
