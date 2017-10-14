using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Behaivor;
using LunaBot.Commands;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LunaBot
{
    class Engine
    {
        private IDictionary<string, BaseCommand> commandDictionary;
        
        private readonly DiscordSocketClient client;

        public SocketGuild guild;
        public SocketTextChannel lobby;
        public List<SocketRole> roles;

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
            client.UserJoined += UserJoined;
       
            this.RegisterCommands();

            client.Ready += Ready;

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

        private async Task Ready()
        {
            guild = client.GetGuild(324967746465169410);
            lobby = client.GetChannel(343193171431522304) as SocketTextChannel;
            roles = guild.Roles.ToList();
        }
        private async Task UserJoined(SocketUser user)
        {
            Logger.Info("System", $"User {user.Username}<{user.Id}> joined the server.");
            if (lobby == null)
                lobby = client.GetChannel(343193171431522304) as SocketTextChannel;
            
            Logger.Info("System", $"Placing {user.Username}<{user.Id}> through tutorial...");
            if(!await StartTutorial(user as SocketGuildUser))
                Logger.Info("System", $"User {user.Username}<{user.Id}> failed the tutorial.");
            
            await lobby.SendMessageAsync($"Welcome {user.Mention} to the server!");
        }

        private async Task MessageReceived(SocketMessage message)
        {
            // ignore your own message if you ever manage to do this.
            if(message.Author.IsBot)
            {
                return;
            }

            // Handle tutorial messages that cannot run commands.
            if(message.Channel.Name.Contains("intro"))
            {

            }

            // Handle commands within the public text channels.
            try
            {
                string messageText = message.Content;
                if (messageText.StartsWith("!"))
                {
                    this.ProcessCommand(message);
                }
                else if (messageText.StartsWith("+"))
                {
                    this.ProcessSetAttribute(message);
                }
                else if (messageText.StartsWith("?"))
                {
                    this.ProcessGetAttribute(message);
                }
                else
                {
                    //FunEmojis(message);
                    await ProcessXpAsync(message);
                    await message.Log();
                }
                
            }
            catch(Exception e)
            {
                e.Log(message);
            }
        }

        private async Task ProcessXpAsync(SocketMessage message)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                User user = db.Users.Where(x => x.DiscordId == userId).SingleOrDefault();

                int oldLevel = user.GetLevel();
                user.Xp++;

                if (user.GetLevel() != oldLevel)
                {
                    IDMChannel channel = await message.Author.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(string.Format("Congrats!! You've leveled up. You're now level {0}", user.GetLevel()));
                }

                db.Users.Attach(user);
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
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

        private void ProcessSetAttribute(SocketMessage message)
        {
            // Cut up the message with the relevent parts
            string messageText = message.Content;
            string[] commandPts = messageText.Substring(1).Split(new Char[] {' '}, 2);
            string command = commandPts[0].ToLower();

            string content = string.Empty;
            if(commandPts.Count() != 1)
            {
                content = commandPts[1];
            }

            this.commandDictionary["set"].Process(message, new[] { command, content });
        }

        private void ProcessGetAttribute(SocketMessage message)
        {
            // Cut up the message with the relevent parts
            string messageText = message.Content;
            string[] commandPts = messageText.Substring(1).Split(new Char[] { ' ' }, 2);
            string command = commandPts[0].ToLower();

            string user = message.Author.Id.ToString();
            SocketUser mentionedUser = message.MentionedUsers.FirstOrDefault();
            if (mentionedUser != null)
            {
                user = mentionedUser.Id.ToString();
            }

            this.commandDictionary["get"].Process(message, new[] { command, user });
        }

        private async void FunEmojis(SocketMessage message)
        {
            //Javi
            if (message.Author.Id == 123470919535427584)
            {
                var m = (RestUserMessage)await message.Channel.GetMessageAsync(message.Id);
                await m.AddReactionAsync(new Emoji("🇲🇽"));
            }
            // Ben
            else if (message.Author.Id == 199271133764255745)
            {
                var m = (RestUserMessage)await message.Channel.GetMessageAsync(message.Id);
                await m.AddReactionAsync(new Emoji("🇮🇱"));
            }
            // Tim
            else if (message.Author.Id == 199864978264686592)
            {
                var m = (RestUserMessage)await message.Channel.GetMessageAsync(message.Id);
                await m.AddReactionAsync(new Emoji("👓"));
            }
            // Mat
            else if (message.Author.Id == 142394877219438592)
            {
                var m = (RestUserMessage)await message.Channel.GetMessageAsync(message.Id);
                await m.AddReactionAsync(new Emoji("🚴"));
            }
            // Tom
            else if (message.Author.Id == 199270845930012672)
            {
                var m = (RestUserMessage)await message.Channel.GetMessageAsync(message.Id);
                await m.AddReactionAsync(new Emoji("☕"));
                await m.AddReactionAsync(new Emoji("🍺"));
            }
        }

        /// <summary>
        /// Sets newbie role to user and places them through the tutorial.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<bool> StartTutorial(SocketGuildUser user)
        {
            Predicate<SocketRole> newbieFinder = (SocketRole sr) => { return sr.Name == "newbie"; };
            SocketRole newbie = roles.Find(newbieFinder);
            await user.AddRoleAsync(newbie);
            
            RestTextChannel introRoom = await guild.CreateTextChannelAsync($"intro-{user.Id}");
            SocketGuildChannel introChannel = guild.GetChannel(introRoom.Id);

            RestUserMessage msg;

            // Start interaction with user.
            await introRoom.SendMessageAsync("Welcome to the server! Lets get you settled alright?");
            Thread.Sleep(2000);
            await introRoom.SendMessageAsync("Firstly, what should we call you?");
            

            await introRoom.DeleteAsync();
            
            return true;
        }

    }
}
