using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Commands;
using LunaBot.Database;
using LunaBot.ServerUtilities;
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
        private IDictionary<ulong, DateTime> messageTimestamps;

        private OverwritePermissions removeAllPerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
        private OverwritePermissions userPerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);


        private readonly DiscordSocketClient client;

        public SocketGuild guild;
        public SocketTextChannel lobby;
        public List<SocketRole> roles;
        public BotReporting report;

        public Engine()
        {
            this.client = new DiscordSocketClient();
            this.commandDictionary = new Dictionary<string, BaseCommand>();
            this.messageTimestamps = new Dictionary<ulong, DateTime>();
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
            report = new BotReporting(guild.GetChannel(328204763965423617));
        }

        private async Task UserJoined(SocketUser user)
        {
            Logger.Info("System", $"User {user.Username}<{user.Id}> joined the server.");
            if (lobby == null)
                lobby = client.GetChannel(343193171431522304) as SocketTextChannel;
            
            Logger.Info("System", $"Placing {user.Username}<{user.Id}> through tutorial...");
            if (!await StartTutorial(user as SocketGuildUser))
                Logger.Warning("System", $"User {user.Username} failed tutorial.");
            
            // await lobby.SendMessageAsync($"Welcome {user.Mention} to the server!");
        }

        private async Task MessageReceived(SocketMessage message)
        {
            //Anti-raid system
            if (await ProcessMessage(message))
                return;

            // ignore your own message if you ever manage to do this.
            if (message.Author.IsBot)
            {
                return;
            }

            // Handle tutorial messages that cannot run commands.
            if(message.Channel.Name.Contains("intro"))
            {
                // check user, check provided input, fill in details
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
                
                // No XP gain if you only say 2 words or less.
                int words = (message.Content.Split(' ').Count<string>());

                if (words < 3)
                    return;
                
                // Adds characters (no whitespace) as XP. Returns true if user leveled up.
                if (user.AddXP(message.Content.Count(c => !Char.IsWhiteSpace(c))))
                {
                    await message.Channel.SendMessageAsync($"Congrats <@{user.DiscordId}>! You leveled up to {user.Level}! :confetti_ball:");
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

            this.commandDictionary[command].Process(message, new[] { command, user });
        }

        /// <summary>
        /// Processes messages and prevents raids by checking the newest message sent by the user and deletes if it doesn't pass criteria
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if deleted, false otherwise.</returns>
        private async Task<bool> ProcessMessage(SocketMessage message)
        {
            ulong user = message.Author.Id;
            DateTime userTimestamp = message.Timestamp.DateTime;
            DateTime cachedTimestamp;

            if(messageTimestamps.TryGetValue(user, out cachedTimestamp))
            {
                if (userTimestamp.Subtract(cachedTimestamp) < TimeSpan.FromSeconds(2))
                {
                    Logger.Info("System", $"User {message.Author.Username}<{message.Author.Id}> is talking too fast. Deleting latest message.");
                    await message.DeleteAsync();

                    return true;
                }
                else
                {
                    messageTimestamps[user] = userTimestamp;
                    return false;
                }
            }
            else
            {
                messageTimestamps.Add(user, DateTime.Now);

                return false;
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
            Predicate<SocketRole> everyoneFinder = (SocketRole sr) => { return sr.Name == "@everyone"; };
            SocketRole newbie = roles.Find(newbieFinder);
            SocketRole everyone = roles.Find(everyoneFinder);

            await user.AddRoleAsync(newbie);

            // Creat intro room
            RestTextChannel introRoom = await guild.CreateTextChannelAsync($"intro-{user.Id}");

            // Make room only visible to new user and admins
            await introRoom.AddPermissionOverwriteAsync(user, userPerm);
            await introRoom.AddPermissionOverwriteAsync(everyone, removeAllPerm);

            // Start interaction with user. Sleeps are for humanizing the bot.
            Thread.Sleep(2000);
            await introRoom.SendMessageAsync("Welcome to the server! Lets get you settled, alright?");
            Thread.Sleep(2000);
            await introRoom.SendMessageAsync("Firstly, what should we call you?");

            // Register user in database
            RegisterCommand registerCommand = new RegisterCommand();
            return registerCommand.manualRegister(user);
            
            //await introRoom.DeleteAsync();

            //RestTextChannel personalRoom = await guild.CreateTextChannelAsync($"room-{user.Id}");
            //await personalRoom.SendMessageAsync("This is your room, you can do whatever you want in here. Try using the !help command to start ;)");
            
        }

    }
}
