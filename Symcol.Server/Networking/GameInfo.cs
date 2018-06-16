using System.Collections.Generic;
using Symcol.Core.Networking;

namespace Symcol.Server.Networking
{
    public class GameInfo
    {
        public string GameID;

        public List<ClientInfo> GameClients = new List<ClientInfo>();
    }
}
