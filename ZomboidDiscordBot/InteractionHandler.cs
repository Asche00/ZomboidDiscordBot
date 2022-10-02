using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using System.Reflection;
using ZomboidDiscordBot.Server;

namespace ZomboidDiscordBot
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private readonly IConfigurationRoot _config;

        // Using constructor injection
        public InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services, IConfigurationRoot config)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }
        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                switch (arg.Type)
                {
                    case InteractionType.MessageComponent:
                        await HandleComponent(arg);
                        break;

                    default:
                        // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                        var ctx = new SocketInteractionContext(_client, arg);
                        await _commands.ExecuteCommandAsync(ctx, _services);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private async Task HandleComponent(SocketInteraction arg)
        {
            try
            {
                var parsedArg = (SocketMessageComponent) arg;

                //Server restart button
                if (parsedArg.Data.CustomId == "restart-yes")
                {
                    var ServerUtil = _services.GetRequiredService<ServerUtility>();

                    if (ServerUtil.GetPlayerCount() > 0)
                    {
                        await parsedArg.UpdateAsync(x =>
                        {
                            x.Content = "There are currently players online. Restart will occur five minutes from now.";
                            x.Components = null;
                        });
                    }
                    else
                    {

                        await parsedArg.UpdateAsync(x =>
                        {
                            x.Content = "Server restart command sent. Please wait for the server to come online again.";
                            x.Components = null;
                        });
                    }
                }

                //Server restart button cancel
                if (parsedArg.Data.CustomId == "restart-no")
                {
                    await parsedArg.UpdateAsync(x =>
                    {
                        x.Content = "Restart cancelled.";
                        x.Components = null;
                    });
                }

                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.MessageComponent)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
