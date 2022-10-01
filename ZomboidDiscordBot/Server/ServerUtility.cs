using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZomboidDiscordBot.Log;
using CoreRCON;
using CoreRCON.Parsers.Standard;
using System.Net;

namespace ZomboidDiscordBot.Server
{
    public class ServerUtility
    {
        private readonly IConfigurationRoot _config;

        public ServerUtility(IConfigurationRoot config)
        {
            _config = config;
        }

        public async Task RestartServerRcon()
        {
            //Get config
            string serverIp = _config["serverinfo:serverip"].ToString();
            ushort rconport = Convert.ToUInt16(_config["serverinfo:rconport"].ToString());
            string rconpassword = _config["serverinfo:rconpassword"].ToString();

            // Connect to a server
            var rcon = new RCON(IPAddress.Parse(serverIp), rconport, rconpassword);
            await rcon.ConnectAsync();

            string response = await rcon.SendCommandAsync("quit");
        }
    }
}
