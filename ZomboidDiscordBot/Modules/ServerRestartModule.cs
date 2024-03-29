﻿using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using ZomboidDiscordBot.Log;
using Discord.WebSocket;
using YamlDotNet.Core.Tokens;
using Newtonsoft.Json.Linq;
using System.Reactive;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZomboidDiscordBot.Server;

namespace ZomboidDiscordBot
{
    // Must use InteractionModuleBase<SocketInteractionContext> for the InteractionService to auto-register the commands
    public class ServerRestartModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private static Logger _logger;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _services;

        public ServerRestartModule(ConsoleLogger logger, IConfigurationRoot config, IServiceProvider services)
        {
            _logger = logger;
            _config = config;
            _services = services;
        }


        // Slash command for server restart
        [SlashCommand("restartserver", "Restart the project zomboid server.")]
        public async Task RestartServer()
        {

            //Check when last update was

            DateTime _lastRestart = DateTime.Parse(_config["data:lastrestart"]);
            int _restartCooldown = Convert.ToInt32(_config["restartcooldown"]);
            int lastRestartMinutes = Convert.ToInt32((DateTime.Now - _lastRestart).TotalMinutes);
            
            //If we restarted recently, do not give the option.
            if (lastRestartMinutes < _restartCooldown)
            {
                await RespondAsync($"Server restart was requested {lastRestartMinutes} minutes ago. Please wait {_restartCooldown} minutes between restart requests.", ephemeral: true);
                return;
            }

            var builder = new ComponentBuilder()
            .WithButton("Restart", "restart-yes", ButtonStyle.Success)
            .WithButton("Cancel", "restart-no", ButtonStyle.Primary);

            await RespondAsync("Are you sure you wish to restart the server?", components: builder.Build(), ephemeral: true);
        }

        // This is the handler is server restart is confirmed
        [ComponentInteraction("restart-yes")]
        public async Task RestartYesHandler()
        {
            var ServerUtil = _services.GetRequiredService<ServerUtility>();

            //If there are players online, delay the restart.
            if (ServerUtil.GetPlayerCount() > 0)
            {
                ServerUtil.HandleDelayedRestart();
            }
            else
            {
                //Do RCON stuff here
                ServerUtil.RestartServerRcon();
            }

            //Save to config file for application restarts
            //This might cause a problem if the server doesn't actually restart for some reason
            //but manual intervention would likely be needed in this case anyway.
            string configfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZomboidBot");
            configfilePath = Path.Combine(configfilePath, "config.yml");

            var deserializer = new YamlDotNet.Serialization.Deserializer();

            using var reader = new StreamReader(configfilePath);
            var obj = deserializer.Deserialize<Dictionary<object, object>>(reader);
            var data = (Dictionary<object, object>)obj["data"];
            reader.Close();

            data["lastrestart"] = DateTime.Now.ToString();
            data["restartuser"] = Context.User.Username;

            //save changes
            using var writer = new StreamWriter(configfilePath);
            var serializer = new YamlDotNet.Serialization.Serializer();
            serializer.Serialize(writer, obj);
            writer.Close();


            //update the config in memory
            _config.Reload();

            await _logger.Log(new LogMessage(LogSeverity.Info, "ServerRestartModule : RestartServer", $"User: {Context.User.Username}, Command: RestartServer", null));
        }
    }
}
