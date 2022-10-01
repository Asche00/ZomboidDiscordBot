using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZomboidDiscordBot.Log;
using CoreRCON;
using CoreRCON.Parsers.Standard;
using System.Net;
using SteamQueryNet;
using SteamQueryNet.Interfaces;
using SteamQueryNet.Models;
using Microsoft.Extensions.Hosting;
using Player = SteamQueryNet.Models.Player;
using Discord.WebSocket;

namespace ZomboidDiscordBot.Server
{
    public class ServerUtility
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public ServerUtility(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
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

        public async Task QueryServerInfo()
        {
            string serverIp = _config["serverinfo:serverip"].ToString();
            ushort serverPort = Convert.ToUInt16(_config["serverinfo:serverport"]);
            IServerQuery serverQuery = new SteamQueryNet.ServerQuery();

            try
            {
                serverQuery.Connect(serverIp, serverPort);
            }
            catch
            {
                await _client.SetGameAsync ("Offline");
                return;
            }

            ServerInfo serverInfo = serverQuery.GetServerInfo();
            List<Player> players = serverQuery.GetPlayers();

            await _client.SetGameAsync($"Online - Players: {players.Count()}");
            return;

        }


    }
}
