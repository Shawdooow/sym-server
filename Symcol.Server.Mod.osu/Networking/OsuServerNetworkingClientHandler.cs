using System.Collections.Generic;
using osu.Framework.Logging;
using Symcol.Core.Networking;
using Symcol.Core.Networking.Packets;
using Symcol.osu.Mods.Multi.Networking;
using Symcol.osu.Mods.Multi.Networking.Packets;
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
                    if (joinPacket.MatchInfo == null)
                        break;

                    MatchListPacket.MatchInfo match = GetMatch(joinPacket.MatchInfo);

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
                case ChatPacket chat:
                    break;
                case LeavePacket leave:
                    if (GetMatch(leave.Match) != null)
                        foreach (OsuClientInfo player in GetMatch(leave.Match).Players)
                            if (player.UserID == leave.Player.UserID)
                            {
                                GetMatch(leave.Match).Players.Remove(player);
                                break;
                            }

                    Logger.Log("Couldn't find a player to remove who told us they were leaving!", LoggingTarget.Network, LogLevel.Error);
                    break;
                //case DeleteMatchPacket deleteMatch:
            }
        }

        protected void ShareWithMatchClients(MatchListPacket.MatchInfo match, Packet packet)
        {
            foreach (OsuClientInfo player in match.Players)
                GetNetworkingClient(player).SendPacket(packet);
        }

        protected MatchListPacket.MatchInfo GetMatch(MatchListPacket.MatchInfo match)
        {
            foreach (MatchListPacket.MatchInfo m in Matches)
                if (m.BeatmapTitle == match.BeatmapTitle &&
                    m.BeatmapArtist == match.BeatmapArtist &&
                    m.Name == match.Name &&
                    m.Username == match.Username)
                    return m;
            return null;
        }
    }
}
