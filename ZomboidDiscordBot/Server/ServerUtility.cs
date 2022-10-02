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
using Discord;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace ZomboidDiscordBot.Server
{
    public class ServerUtility
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;
        private CancellationTokenSource? updateStatusCts;
        private static Logger _logger;
        private int PlayerCount = 0;

        public ServerUtility(IConfigurationRoot config, DiscordSocketClient client, ConsoleLogger logger)
        {
            _config = config;
            _client = client;
            _logger = logger;
        }


        public async Task HandleDelayedRestart()
        {

            await AnnounceServerRcon("Restart requested. Server will restart in 5 minutes.");
            // Sleeptime in milliseconds 
            // 4 minutes
            int sleepTime = (4 * 60 * 1000);


            Thread.Sleep(sleepTime);
            await AnnounceServerRcon("Restart pending. Server will restart in 1 minute.");

            // Sleeptime in milliseconds 
            // 1 minute
            sleepTime = (1 * 60 * 1000);
            Thread.Sleep(sleepTime);

            await RestartServerRcon();
        }

        public async Task AnnounceServerRcon(string MsgText)
        {
            //Get config
            string serverIp = _config["serverinfo:serverip"].ToString();
            ushort rconport = Convert.ToUInt16(_config["serverinfo:rconport"].ToString());
            string rconpassword = _config["serverinfo:rconpassword"].ToString();

            // Connect to a server
            var rcon = new RCON(IPAddress.Parse(serverIp), rconport, rconpassword);
            await rcon.ConnectAsync();

            string response = await rcon.SendCommandAsync($"servermsg \"{MsgText}\" ");
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


        public int GetPlayerCount()
        {
            return PlayerCount;
        }

        public async Task QueryServerInfo()
        {
            updateStatusCts = new();
            while (!updateStatusCts.IsCancellationRequested)
            {
                string serverIp = _config["serverinfo:serverip"].ToString();
                ushort serverPort = Convert.ToUInt16(_config["serverinfo:serverport"]);
                int receiveTimeout = Convert.ToInt32(_config["serverinfo:receivetimeout"]) * 1000;
                int sendTimeout = Convert.ToInt32(_config["serverinfo:sendtimeout"]) * 1000;

                IServerQuery serverQuery = new SteamQueryNet.ServerQuery();

                try
                {
                    //await _logger.Log(new LogMessage(LogSeverity.Debug, $"Querying server", null));
                    serverQuery.Connect(serverIp, serverPort, receiveTimeout, sendTimeout);

                    //ServerInfo serverInfo = serverQuery.GetServerInfo();
                    Task<List<Player>> players = serverQuery.GetPlayersAsync(updateStatusCts.Token);
                    if (!players.Wait(5000))
                    {
                        players.Dispose();
                        throw new Exception();
                    }

                    await _client.SetActivityAsync(new Game($"Zomboid with {players.Result.Count()} players")).ConfigureAwait(false);
                    await _logger.Log(new LogMessage(LogSeverity.Info, $"Players: {players.Result.Count()}", null));
                    PlayerCount = players.Result.Count();
                    players.Dispose();
                }
                catch
                {
                    await _client.SetActivityAsync(new Game("Offline")).ConfigureAwait(false);
                    await _logger.Log(new LogMessage(LogSeverity.Info, $"Server offline", null));
                }

                await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
            }

        }


    }
}
