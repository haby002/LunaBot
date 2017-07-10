using Discord;
using Discord.WebSocket;
using LunaBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot
{
    class Engine
    {
        private IDictionary<string, BaseCommand> commandDictionary;

        private readonly DiscordSocketClient client;

        public Engine()
        {
            this.client = new DiscordSocketClient();
            this.commandDictionary = new Dictionary<string, BaseCommand>();
        }

        public async Task Run()
        {
            client.Log += Logger.Log;

            string token = SecretStrings.token;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            client.MessageReceived += MessageReceived;

            this.RegisterCommands();

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
            }
        }

        private async Task MessageReceived(SocketMessage message)
        {
            string messageText = message.Content;
            if(messageText.StartsWith("!"))
            {
                this.ProcessCommand(message);
            }
            if(messageText.StartsWith("+"))
            {
                this.ProcessAttribute(message);
            }
            else
            {
                await message.Log();
            }
        }

        private void ProcessCommand(SocketMessage message)
        {
            // Cut up the message with the relevent parts
            string messageText = message.Content;
            string[] commandPts = messageText.Substring(1).Split(' ');
            string command = commandPts[0].ToLower();
            List<string> commandParamsList = new List<string>(commandPts);
            commandParamsList.RemoveAt(0);
            string[] commandParams = commandParamsList.ToArray();

            if (this.commandDictionary.ContainsKey(command))
            {
                Logger.Verbose(
                    message.Author.Username, 
                    string.Format(StringTable.RecognizedCommand, command, string.Join(",", commandParams)));
                try
                {
                    this.commandDictionary[command].Process(message, commandParams);
                }
                catch (Exception e)
                {
                    message.Channel.SendMessageAsync(string.Format("Command failed: {0}", e.Message));
                    while (e.InnerException != null)
                    {
                        e = e.InnerException;
                        message.Channel.SendMessageAsync(string.Format("Command failed: {0}", e.Message));
                    }
                }

                return;
            }
            else
            {
                Logger.Error(message.Author.Username, string.Format(StringTable.UnrecognizedCommand, command));
            }
        }

        private void ProcessAttribute(SocketMessage message)
        {
            // Cut up the message with the relevent parts
            string messageText = message.Content;
            string[] commandPts = messageText.Substring(1).Split(new Char[] {' '}, 2);
            string command = commandPts[0].ToLower();
            string content = commandPts[1];

            if (this.commandDictionary.ContainsKey(command))
            {
                Logger.Verbose(
                    message.Author.Username,
                    string.Format(StringTable.RecognizedCommand, command, string.Join(",", content)));
                try
                {
                    this.commandDictionary[command].Process(message, new String[] { content });
                }
                catch (Exception e)
                {
                    message.Channel.SendMessageAsync(string.Format("Command failed: {0}", e.Message));
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
