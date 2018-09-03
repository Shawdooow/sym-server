using System.Collections.Generic;
using osu.Framework.Logging;
using Symcol.Networking.NetworkingHandlers.Server;
using Symcol.Networking.Packets;
using Symcol.osu.Mods.Multi.Networking;
using Symcol.osu.Mods.Multi.Networking.Packets.Lobby;
using Symcol.osu.Mods.Multi.Networking.Packets.Match;
using Symcol.osu.Mods.Multi.Networking.Packets.Player;

namespace Symcol.Server.Mod.osu.Networking
{
    public class OsuServerNetworkingClientHandler : ServerNetworkingHandler
    {
        protected override string Gamekey => "osu";

        protected readonly List<ServerMatch> ServerMatches = new List<ServerMatch>();

        protected override void HandlePackets(Packet packet)
        {
            Logger.Log($"Recieved a Packet from {NetworkingClient.EndPoint}", LoggingTarget.Network, LogLevel.Debug);

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
                    if (joinPacket.OsuUserInfo == null)
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
                        match.Users.Add(joinPacket.OsuUserInfo);

                        foreach (ServerMatch s in ServerMatches)
                            if (s.MatchInfo == match)
                                s.Players.Add(new Player
                                {
                                    OsuUserInfo = joinPacket.OsuUserInfo,
                                    PlayerLastUpdateTime = Time.Current
                                });

                        //Tell them they have joined
                        SendToClient(new JoinedMatchPacket { Users = match.Users }, joinPacket);

                        //Tell everyone already there someone joined
                        ShareWithMatchClients(match, new PlayerJoinedPacket
                        {
                            User = joinPacket.OsuUserInfo
                        });
                    }
                    else
                        Logger.Log("Couldn't find a match matching one in packet!", LoggingTarget.Network, LogLevel.Error);

                    break;
                case GetMapPacket getMap:
                    match = GetMatch(getMap.User);
                    NetworkingClient.SendPacket(SignPacket(new SetMapPacket
                    {
                        OnlineBeatmapSetID = match.OnlineBeatmapSetID,
                        OnlineBeatmapID = match.OnlineBeatmapID,
                        BeatmapTitle = match.BeatmapTitle,
                        BeatmapArtist = match.BeatmapArtist,
                        BeatmapMapper = match.BeatmapMapper,
                        BeatmapDifficulty = match.BeatmapDifficulty,
                        RulesetID = match.RulesetID,
                    }), GetClient(getMap).EndPoint);
                    break;
                case SetMapPacket map:
                    match = GetMatch(map.User);

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
                    ShareWithMatchClients(GetMatch(chat.User), chat);
                    break;
                case LeavePacket leave:
                    if (GetMatch(leave.User) != null)
                        foreach (OsuUserInfo player in GetMatch(leave.User).Users)
                            if (player.UserID == leave.User.UserID)
                            {
                                GetMatch(leave.User).Users.Remove(player);

                                foreach (ServerMatch m in ServerMatches)
                                    foreach (Player p in m.LoadedPlayers)
                                        if (p.OsuUserInfo.UserID == leave.User.UserID)
                                        {
                                            m.Players.Remove(p);
                                            m.LoadedPlayers.Remove(p);
                                            break;
                                        }

                                //Update their matchlist next
                                MatchListPacket list = new MatchListPacket();
                                list = (MatchListPacket)SignPacket(list);
                                list.MatchInfoList = GetMatches();
                                NetworkingClient.SendPacket(list, GetClient(leave).EndPoint);
                                break;
                            }

                    Logger.Log("Couldn't find a player to remove who told us they were leaving!", LoggingTarget.Network, LogLevel.Error);
                    break;
                case StartMatchPacket start:
                    match = GetMatch(start.User);
                    ShareWithMatchClients(match, new MatchLoadingPacket
                    {
                        Users = match.Users
                    });
                    break;
                case PlayerLoadedPacket loaded:
                    foreach (ServerMatch m in ServerMatches)
                        foreach (Player p in m.Players)
                            if (p.OsuUserInfo.UserID == loaded.User.UserID)
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
                            if (p.OsuUserInfo.UserID == exit.User.UserID)
                            {
                                restart:
                                foreach (Player r in m.LoadedPlayers)
                                {
                                    m.LoadedPlayers.Remove(r);
                                    m.Players.Add(r);
                                    goto restart;
                                }
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

                if (match.MatchInfo.Users.Count == 0 && match.MatchLastUpdateTime + 60000 <= Time.Current)
                {
                    ServerMatches.Remove(match);
                    Logger.Log("Empty match deleted!");
                    goto restart;
                }

                if (match.MatchInfo.Users.Count > 0)
                {
                    match.MatchLastUpdateTime = Time.Current;
                }
            }
        }

        protected void ShareWithMatchClients(MatchListPacket.MatchInfo match, Packet packet)
        {
            foreach (OsuUserInfo player in match.Users)
                NetworkingClient.SendPacket(packet, GetClient(player).EndPoint);
        }

        protected Player GetPlayer(OsuUserInfo client)
        {
            foreach (ServerMatch m in ServerMatches)
                foreach (Player p in m.Players)
                    if (p.OsuUserInfo.UserID == client.UserID)
                        return p;
            return null;
        }

        protected MatchListPacket.MatchInfo GetMatch(OsuUserInfo player)
        {
            foreach (MatchListPacket.MatchInfo m in GetMatches())
                foreach (OsuUserInfo p in m.Users)
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

        protected List<OsuUserInfo> GetOsuClientInfos(ServerMatch serverMatch)
        {
            List<OsuUserInfo> clients = new List<OsuUserInfo>();

            foreach (Player player in serverMatch.Players)
                clients.Add(player.OsuUserInfo);

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
            public OsuUserInfo OsuUserInfo;

            public double PlayerLastUpdateTime;
        }
    }
}
