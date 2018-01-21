using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Commands;
using LunaBot.ServerUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace LunaBot
{
    class Engine
    {
        private IDictionary<string, BaseCommand> commandDictionary;
        private IDictionary<string, string> aliasDictionary;
        private IDictionary<ulong, DateTime> messageTimestamps;

        
        public static SocketGuildUser luna;

        private readonly DiscordSocketClient client;

        public SocketGuild guild;
        public SocketTextChannel lobby;
        public List<SocketRole> roles;
        public BotReporting report;

        public Engine()
        {
            this.client = new DiscordSocketClient();
            this.commandDictionary = new Dictionary<string, BaseCommand>();
            this.aliasDictionary = new Dictionary<string, string>();
            this.messageTimestamps = new Dictionary<ulong, DateTime>();
        }

        public async Task RunAsync()
        {
            client.Log += Logger.LogAsync;

            string token = SecretStrings.token;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            
            client.MessageReceived += MessageReceivedAsync;
            
            this.RegisterCommands();

            client.Ready += ReadyAsync;

            await Task.Delay(-1);
        }

        /// <summary>
        /// Registers all commands in LunaBot.Commands namespace
        /// </summary>
        private void RegisterCommands()
        {
            Type[] commands = Assembly.GetExecutingAssembly().GetTypes().Where(t => String.Equals(t.Namespace, "LunaBot.Commands", StringComparison.Ordinal)).ToArray();
            commands = commands.Where(x => x.GetCustomAttributes(typeof(LunaBotCommandAttribute)).Any()).ToArray();

            foreach(Type command in commands)
            {
                LunaBotCommandAttribute commandAttribute = command.GetCustomAttribute(typeof(LunaBotCommandAttribute)) as LunaBotCommandAttribute;
                this.commandDictionary[commandAttribute.Name] = Activator.CreateInstance(command) as BaseCommand;
                
                foreach(string alias in commandAttribute.Aliases)
                {
                    this.aliasDictionary[alias] = commandAttribute.Name;
                }
            }
        }
        
        private async Task ReadyAsync()
        {
            guild = client.GetGuild(Guilds.Guild);
            await guild.DownloadUsersAsync();
            lobby = client.GetChannel(Channels.Lobby) as SocketTextChannel;
            roles = guild.Roles.ToList();
            report = new BotReporting(guild.GetChannel(Channels.BotLogs));
            luna = guild.GetUser(UserIds.Luna);
            

            // Set Playing flavor text
            await client.SetGameAsync("!help");

            await LobbyAnnouncements.StartupConfirmationAsync(lobby);

            // Remove all mute from muted users
            
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            try
            {
                // Log Message
                await message.LogAsync(LogSeverity.Verbose).ConfigureAwait(false);

                // ignore your own message if you ever manage to do this.
                if (message.Author.IsBot)
                {
                    return;
                }


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() =>
                {
                    // Commands
                    string messageText = message.Content;

                    if (messageText.StartsWith("k"))
                    {
                        ProcessCommandAsync(message).ConfigureAwait(false);
                    }
                    else
                    {
                        return;
                    }
                }).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            }
            catch (Exception e)
            {
                e.Log(message);
            }

        }
        
        private async Task ProcessCommandAsync(SocketMessage message)
        {
            // Cut up the message with the relevent parts
            string messageText = message.Content;
            string[] commandPts = messageText.Substring(1).Split(new Char[] { ' ' }, 4);
            string command = commandPts[0].ToLower();
            List<string> commandParamsList = new List<string>(commandPts);
            commandParamsList.RemoveAt(0);
            string[] commandParams = commandParamsList.ToArray();

            if(this.aliasDictionary.ContainsKey(command))
            {
                command = this.aliasDictionary[command];
            }

            if (this.commandDictionary.ContainsKey(command))
            {
                Logger.Verbose(
                    message.Author.Username, 
                    string.Format(StringTable.RecognizedCommand, command, string.Join(",", commandParams)));
                try
                {
                    await this.commandDictionary[command].ProcessAsync(message, commandParams);
                }
                catch (Exception e)
                {
                    Logger.Error("system", string.Format("Command failed: {0}", e.Message));
                    while (e.InnerException != null)
                    {
                        e = e.InnerException;
                        Logger.Error("system", string.Format("Command failed: {0}", e.Message));
                    }

                    await message.Channel.SendMessageAsync("Command failed. Try using !help for more info.`");
                }

                return;
            }
            else
            {
                Logger.Error(message.Author.Username, string.Format(StringTable.UnrecognizedCommand, command));
            }
        }
    }
}
