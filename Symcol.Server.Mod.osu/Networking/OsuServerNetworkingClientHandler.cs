using System.Collections.Generic;
using osu.Framework.Logging;
using Symcol.Core.Networking.Packets;
using Symcol.osu.Mods.Multi.Networking;
using Symcol.osu.Mods.Multi.Networking.Packets.Lobby;
using Symcol.osu.Mods.Multi.Networking.Packets.Match;
using Symcol.osu.Mods.Multi.Networking.Packets.Player;
using Symcol.Server.Networking;

namespace Symcol.Server.Mod.osu.Networking
{
    public class OsuServerNetworkingClientHandler : ServerNetworkingClientHandler
    {
        protected override string Gamekey => "osu";

        //TODO: if a match is empty for 1 min delete it automatically
        protected readonly List<ServerMatch> ServerMatches = new List<ServerMatch>();

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
                        MatchInfoList = GetMatches()
                    };
                    SendToClient(matchList, getMatch);
                    break;
                case CreateMatchPacket createMatch:
                    ServerMatches.Add(new ServerMatch
                    {
                        MatchInfo = createMatch.MatchInfo,
                        MatchLastUpdateTime = Time.Current
                    });
                    SendToClient(new MatchCreatedPacket{ MatchInfo = createMatch.MatchInfo }, createMatch);
                    break;
                case JoinMatchPacket joinPacket:
                    if (joinPacket.OsuClientInfo == null)
                        break;

                    foreach (MatchListPacket.MatchInfo m in GetMatches())
                        if (m.BeatmapTitle == joinPacket.Match.BeatmapTitle &&
                            m.BeatmapArtist == joinPacket.Match.BeatmapArtist &&
                            m.Name == joinPacket.Match.Name &&
                            m.Username == joinPacket.Match.Username)
                            match = m;

                    if (match != null)
                    {
                        //Add them
                        match.Players.Add(joinPacket.OsuClientInfo);

                        foreach (ServerMatch s in ServerMatches)
                            if (s.MatchInfo == match)
                                s.Players.Add(new Player
                                {
                                    OsuClientInfo = joinPacket.OsuClientInfo,
                                    PlayerLastUpdateTime = Time.Current
                                });

                        //Tell them they have joined
                        SendToClient(new JoinedMatchPacket { Players = match.Players }, joinPacket);

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
                case GetMapPacket getMap:
                    match = GetMatch(getMap.Player);
                    GetNetworkingClient(GetClientInfo(getMap)).SendPacket(SignPacket(new SetMapPacket
                    {
                        OnlineBeatmapSetID = match.OnlineBeatmapSetID,
                        OnlineBeatmapID = match.OnlineBeatmapID,
                        BeatmapTitle = match.BeatmapTitle,
                        BeatmapArtist = match.BeatmapArtist,
                        BeatmapMapper = match.BeatmapMapper,
                        BeatmapDifficulty = match.BeatmapDifficulty,
                        RulesetID = match.RulesetID,
                    }));
                    break;
                case SetMapPacket map:
                    match = GetMatch(map.Player);

                    match.OnlineBeatmapSetID = map.OnlineBeatmapSetID;
                    match.OnlineBeatmapID = map.OnlineBeatmapID;
                    match.BeatmapTitle = map.BeatmapTitle;
                    match.BeatmapArtist = map.BeatmapArtist;
                    match.BeatmapMapper = map.BeatmapMapper;
                    match.BeatmapDifficulty = map.BeatmapDifficulty;
                    match.RulesetID = map.RulesetID;

                    ShareWithMatchClients(match, map);
                    break;
                case ChatPacket chat:
                    ShareWithMatchClients(GetMatch(chat.Player), chat);
                    break;
                case LeavePacket leave:
                    if (GetMatch(leave.Player) != null)
                        foreach (OsuClientInfo player in GetMatch(leave.Player).Players)
                            if (player.UserID == leave.Player.UserID)
                            {
                                GetMatch(leave.Player).Players.Remove(player);

                                MatchListPacket list = new MatchListPacket();
                                list = (MatchListPacket)SignPacket(list);
                                list.MatchInfoList = GetMatches();
                                GetNetworkingClient(GetClientInfo(leave)).SendPacket(list);
                                break;
                            }

                    Logger.Log("Couldn't find a player to remove who told us they were leaving!", LoggingTarget.Network, LogLevel.Error);
                    break;
                case StartMatchPacket start:
                    match = GetMatch(start.Player);
                    ShareWithMatchClients(match, new MatchLoadingPacket
                    {
                        Players = match.Players
                    });
                    break;
                case PlayerLoadedPacket loaded:
                    foreach (ServerMatch m in ServerMatches)
                        foreach (Player p in m.Players)
                            if (p.OsuClientInfo.UserID == loaded.Player.UserID)
                            {
                                m.Players.Remove(p);
                                m.LoadedPlayers.Add(p);

                                if (m.Players.Count == 0)
                                    ShareWithMatchClients(m.MatchInfo, new MatchStartingPacket());

                                break;
                            }

                    Logger.Log("A Player we can't find told us they have loaded!", LoggingTarget.Network, LogLevel.Error);
                    break;
                case MatchExitPacket exit:
                    foreach (ServerMatch m in ServerMatches)
                        foreach (Player p in m.LoadedPlayers)
                            if (p.OsuClientInfo.UserID == exit.Player.UserID)
                                foreach (Player r in m.Players)
                                {
                                    m.LoadedPlayers.Remove(r);
                                    m.Players.Add(r);
                                    break;
                                }
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            restart:
            foreach (ServerMatch match in ServerMatches)
            {
                foreach (Player player in match.Players)
                {
                    
                }

                if (match.MatchInfo.Players.Count == 0 && match.MatchLastUpdateTime + 60000 <= Time.Current)
                {
                    ServerMatches.Remove(match);
                    Logger.Log("Empty match deleted!");
                    goto restart;
                }

                if (match.MatchInfo.Players.Count > 0)
                {
                    match.MatchLastUpdateTime = Time.Current;
                }
            }
        }

        protected void ShareWithMatchClients(MatchListPacket.MatchInfo match, Packet packet)
        {
            foreach (OsuClientInfo player in match.Players)
                GetNetworkingClient(player).SendPacket(packet);
        }

        protected MatchListPacket.MatchInfo GetMatch(OsuClientInfo player)
        {
            foreach (MatchListPacket.MatchInfo m in GetMatches())
                foreach (OsuClientInfo p in m.Players)
                    if (p.UserID == player.UserID)
                        return m;
            return null;
        }

        protected List<MatchListPacket.MatchInfo> GetMatches()
        {
            List<MatchListPacket.MatchInfo> matches = new List<MatchListPacket.MatchInfo>();

            foreach (ServerMatch match in ServerMatches)
                matches.Add(match.MatchInfo);

            return matches;
        }

        protected List<OsuClientInfo> GetOsuClientInfos(ServerMatch serverMatch)
        {
            List<OsuClientInfo> clients = new List<OsuClientInfo>();

            foreach (Player player in serverMatch.Players)
                clients.Add(player.OsuClientInfo);

            return clients;
        }

        protected class ServerMatch
        {
            public MatchListPacket.MatchInfo MatchInfo;

            public List<Player> Players = new List<Player>();

            public List<Player> LoadedPlayers = new List<Player>();

            public double MatchLastUpdateTime;
        }

        protected class Player
        {
            public OsuClientInfo OsuClientInfo;

            public double PlayerLastUpdateTime;
        }
    }
}
