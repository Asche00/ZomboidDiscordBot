using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZomboidTypesLibrary;

namespace ZomboidDiscordBot.Server
{
    public static class ServerInfo
    {
        public static ZomboidTypesLibrary.ZomboidTypeServerState.ServerState ServerState = ZomboidTypeServerState.ServerState.Offline;
        public static int PlayerCount = 0;
    }
}
