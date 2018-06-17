using System.Collections.Generic;
using Symcol.Core.Networking;

namespace Symcol.Server.Networking
{
    public class GameInfo
    {
        public string Name;

        public string GameID;

        //TODO: Implement this
        public uint MaxPlayers = 8;

        public List<ClientInfo> GameClients = new List<ClientInfo>();
    }
}
