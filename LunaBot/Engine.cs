using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Commands;
using LunaBot.Database;
using LunaBot.ServerUtilities;
using Microsoft.EntityFrameworkCore;
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

        public static OverwritePermissions removeAllPerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
        public static OverwritePermissions userPerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
        public static OverwritePermissions lunaTutPerm = new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow);

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
            client.UserLeft += UserLeft;
            client.UserBanned += UserBanned;
            
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
            guild = client.GetGuild(Guilds.Guild);
            await guild.DownloadUsersAsync();
            lobby = client.GetChannel(Channels.Lobby) as SocketTextChannel;
            roles = guild.Roles.ToList();
            report = new BotReporting(guild.GetChannel(Channels.BotLogs));
            luna = guild.GetUser(UserIds.Luna);

            // Adding Haby as owner
            using (DiscordContext db = new DiscordContext())
            {
                db.Database.EnsureCreated();
                foreach (ulong userId in UserIds.Owners)
                {
                    User owner = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();
                    if (owner == null)
                    {
                        Logger.Warning("System", "Haby001 not found, adding as Admin.");

                        User newUser = new User();
                        newUser.DiscordId = userId;
                        newUser.Level = 1;
                        newUser.Privilege = User.Privileges.Owner;
                        newUser.TutorialFinished = true;
                        newUser.Gender = User.Genders.Male;

                        db.Users.Add(newUser);
                        db.SaveChanges();

                        Logger.Verbose("System", "Created admin Haby");
                        return;
                    }
                    else if (owner.Privilege != User.Privileges.Owner)
                    {
                        owner.Privilege = User.Privileges.Owner;

                        db.SaveChanges();

                        Logger.Verbose("System", "Updated Haby's priviledges to Owner");
                    }
                }
            }

            // Set Playing flavor text
            await client.SetGameAsync("!help");

            // Remove all mute from muted users
            
        }

        private async Task UserJoined(SocketUser user)
        {
            Logger.Info("System", $"User {user.Username}<{user.Id}> joined the server.");
            if (lobby == null)
                lobby = client.GetChannel(343193171431522304) as SocketTextChannel;

            RegisterCommand registerCommand = new RegisterCommand();

            registerCommand.manualRegister(user as SocketGuildUser);
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = user.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().TutorialFinished)
                {
                    Logger.Info("System", $"{user.Username}<@{user.Id}> already finished the tutorial. Announcing in lobby.");
                    await lobby.SendMessageAsync($"Welcome back <@{user.Id}> to the server!");
                }
                else
                {
                    Logger.Info("System", $"Placing {user.Username}<@{user.Id}> through tutorial...");
                    if (!await StartTutorial(user as SocketGuildUser))
                    {
                        Logger.Warning("System", $"User {user.Username} already registered.");
                    }
                }
            }
            
        }

        private async Task UserLeft(SocketUser user)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = user.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().TutorialFinished)
                {
                    Logger.Info("System", $"User {user.Username}<@{user.Id}> has left the server.");
                    await lobby.SendMessageAsync($"<@{user.Id}> has left the server :wave:");
                }
            }
        }

        private async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            await lobby.SendMessageAsync($"My :banhammer: to your face!");
            Logger.Info("System", $"User {user.Username}<@{user.Id}> has been banned from the server.");
        }

        private async Task MessageReceived(SocketMessage message)
        {
            // Handle commands within the public text channels.
            try
            {
                // Log Message
                await message.Log(LogSeverity.Verbose);

                // ignore your own message if you ever manage to do this.
                if (message.Author.IsBot)
                {
                    return;
                }

                //Anti-raid system
                if (await ProcessMessage(message))
                    return;

                // Tutorial messages that cannot run commands.
                if(message.Channel.Name.Contains("intro"))
                {
                    await ProcessTutorialMessaage(message);
                    return;
                }
            
                // Commands
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
                    ProcessXpAsync(message);
                }

            }
            catch(Exception e)
            {
                e.Log(message);
            }

        }

        private void ProcessXpAsync(SocketMessage message)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                User user = db.Users.Where(x => x.DiscordId == userId).SingleOrDefault();

                // No XP gain if you only say 2 words or less.
                int words = (message.Content.Split(' ').Count<string>());

                if (words < 3)
                    return;

                int xp = message.Content.Split(' ').Select(x => x.Trim()).Where((s) => s.Count() > 2).Sum(x => x.Count());
                // Adds characters (no whitespace) as XP. Returns true if user leveled up.
                if (user.AddXP(xp))
                {
                    if (user.Level % 5 == 0)
                    {
                        message.Channel.SendMessageAsync($"Congrats <@{user.DiscordId}>! You leveled up to {user.Level}! :confetti_ball:");
                    }
                }

                // Updates last time message was recieved.
                user.LastMessage = DateTime.UtcNow;

                db.Users.Attach(user);
                db.SaveChanges();
            }
        }

        private void ProcessCommand(SocketMessage message)
        {
            // Cut up the message with the relevent parts
            string messageText = message.Content;
            string[] commandPts = messageText.Substring(1).Split(new Char[] { ' ' }, 4);
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
                    Logger.Error("system", string.Format("Command failed: {0}", e.Message));
                    while (e.InnerException != null)
                    {
                        e = e.InnerException;
                        Logger.Error("system", string.Format("Command failed: {0}", e.Message));
                    }

                    message.Channel.SendMessageAsync("Command failed. Try using !help for more info.`");
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
            if(commandPts.Count() > 1)
            {
                content = commandPts[1];
            }
            else
            {
                message.Channel.SendMessageAsync("I can't set something as nothing, try `+Attribute <Content>`");
                return;
            }

            try
            {
                this.commandDictionary["set_" + command].Process(message, new[] { content });
            }
            catch (Exception e)
            {
                Logger.Error("system", string.Format("Command failed: {0}", e.Message));
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    Logger.Error("system", string.Format("Command failed: {0}", e.Message));
                }
                message.Channel.SendMessageAsync("Command failed. Try using !help for more info.`");
            }
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
            try
            {
                this.commandDictionary["get_" + command].Process(message, new[] { command, user });
            }
            catch (Exception e)
            {
                Logger.Error("system", string.Format("Command failed: {0}", e.Message));
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    Logger.Error("system", string.Format("Command failed: {0}", e.Message));
                }
                message.Channel.SendMessageAsync("Command failed. Try using !help for more info.`");
            }
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

            // Ignore Luna
            if (message.Author.Id == 333285108402487297)
                return false;
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                User databaseUser = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();
                if ((int)databaseUser.Privilege >= 1)
                    return false;
            }

            if(messageTimestamps.TryGetValue(user, out cachedTimestamp))
            {
                if (userTimestamp.Subtract(cachedTimestamp) < TimeSpan.FromSeconds(1))
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
            Predicate<SocketRole> newbieFinder = (SocketRole sr) => { return sr.Name == "Newbie"; };
            Predicate<SocketRole> everyoneFinder = (SocketRole sr) => { return sr.Name == "@everyone"; };
            SocketRole newbie = roles.Find(newbieFinder);
            SocketRole everyone = roles.Find(everyoneFinder);

            await user.AddRoleAsync(newbie);

            // Creat intro room
            RestTextChannel introRoom = await guild.CreateTextChannelAsync($"intro-{user.Id}");

            // Make room only visible to new user and admins
            await introRoom.AddPermissionOverwriteAsync(user, userPerm);
            //await introRoom.AddPermissionOverwriteAsync(everyone, removeAllPerm);
            //await introRoom.AddPermissionOverwriteAsync(luna, lunaTutPerm);

            // Register user in database
            RegisterCommand registerCommand = new RegisterCommand();
            bool register = registerCommand.manualRegister(user);

            // Start interaction with user. Sleeps are for humanizing the bot.
            await introRoom.SendMessageAsync("Welcome to the server! Lets get you settled, alright?");
            Thread.Sleep(2000);
            await introRoom.SendMessageAsync("Firstly, what should we call you?");

            return register;
            //await introRoom.DeleteAsync();

            //RestTextChannel personalRoom = await guild.CreateTextChannelAsync($"room-{user.Id}");
            //await personalRoom.SendMessageAsync("This is your room, you can do whatever you want in here. Try using the !help command to start ;)");
            
        }

        private async Task ProcessTutorialMessaage(SocketMessage message)
        {
            // Ignore tutorial finished people -
            // Find what step they are on
            // Order is Nickname, Gender, Age, Fur,Description, Ref
            // process response
            //  -> ask next question and fill in data
            // !-> ask to provide a correct answer
            using (DiscordContext db = new DiscordContext())
            {
                SocketGuildUser user = message.Author as SocketGuildUser;
                ulong userId = user.Id;
                User databaseUser = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();

                if (databaseUser.TutorialFinished)
                {
                    Logger.Verbose("System", "Registered user talking in tutorial room.");
                    
                    return;
                }

                if (databaseUser.Nickname == null)
                {
                    SocketGuildUser guildUser = message.Author as SocketGuildUser;
                    await guildUser.ModifyAsync(n => n.Nickname = message.Content);
                    databaseUser.Nickname = message.Content;
                    Logger.Verbose(user.Username, $"Changed nickname from {user.Username} to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync("I've gone ahead and changed your name. Now lets set your gender.\n" +
                        "If you don't want to fill this in you can just choose other." +
                        "You can choose between:\n" +
                        "- Male\n" +
                        "- Female\n" +
                        "- Trans-Female\n" +
                        "- Trans-Male\n" +
                        "- Other");
                }
                else if (databaseUser.Gender == User.Genders.None)
                {
                    Predicate<SocketRole> genderFinder;
                    SocketRole gender;

                    switch (message.Content.ToLower())
                    {
                        case "male":
                        case "m":
                            genderFinder = (SocketRole sr) => { return sr.Name == Roles.Male; };
                            gender = roles.Find(genderFinder);
                            await user.AddRoleAsync(gender);
                            databaseUser.Gender = User.Genders.Male;
                            break;
                        case "female":
                        case "f":
                            genderFinder = (SocketRole sr) => { return sr.Name == Roles.Female; };
                            gender = roles.Find(genderFinder);
                            await user.AddRoleAsync(gender);
                            databaseUser.Gender = User.Genders.Female;
                            break;
                        case "other":
                        case "o":
                            genderFinder = (SocketRole sr) => { return sr.Name == Roles.Other; };
                            gender = roles.Find(genderFinder);
                            await user.AddRoleAsync(gender);
                            databaseUser.Gender = User.Genders.Other;
                            break;
                        case "trans-male":
                            genderFinder = (SocketRole sr) => { return sr.Name == Roles.TransMale; };
                            gender = roles.Find(genderFinder);
                            await user.AddRoleAsync(gender);
                            databaseUser.Gender = User.Genders.TransM;
                            break;
                        case "trans-female":
                            genderFinder = (SocketRole sr) => { return sr.Name == Roles.TransFemale; };
                            gender = roles.Find(genderFinder);
                            await user.AddRoleAsync(gender);
                            databaseUser.Gender = User.Genders.TransF;
                            break;
                        default:
                            await message.Channel.SendMessageAsync("I'm sorry I couldn't understand your message. Make sure it's either `male`, `female`, `trans-male`, `trans-female`, or `other`.");
                            return;
                    }

                    Logger.Verbose(user.Username, $"Setting gender to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync($"Alright, you are now `{message.Content}`. \n" +
                        $"Next lets set your orientation.\n" +
                        "You can choose from below:\n" +
                        "- Straight\n" +
                        "- Gay\n" +
                        "- Bisexual\n" +
                        "- Asexual\n" +
                        "- Pansexual\n" +
                        "- Gray-a (if you'd rather it not be shown)\n");
                }
                else if(databaseUser.orientation == User.Orientation.None)
                {
                    Predicate<SocketRole> orientationFinder;
                    SocketRole orientation;

                    switch (message.Content.ToLower())
                    {
                        case "straight":
                        case "s":
                            orientationFinder = (SocketRole sr) => { return sr.Name == "straight"; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Straight;
                            break;
                        case "gay":
                        case "g":
                            orientationFinder = (SocketRole sr) => { return sr.Name == "gay"; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Gay;
                            break;
                        case "bisexual":
                        case "bi":
                            orientationFinder = (SocketRole sr) => { return sr.Name == "bi"; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Bi;
                            break;
                        case "asexual":
                            orientationFinder = (SocketRole sr) => { return sr.Name == "asexual"; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Asexual;
                            break;
                        case "gray-a":
                            orientationFinder = (SocketRole sr) => { return sr.Name == "gray-a"; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Gray;
                            break;
                        case "pansexual":
                        case "pan":
                            orientationFinder = (SocketRole sr) => { return sr.Name == "pan"; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Pansexual;
                            break;
                        default:
                            await message.Channel.SendMessageAsync("Hmm... That's not an orientation I can undestand.\n" +
                                "Make sure it's either straight, gay, bisexaul, asexual, or gray-a.");
                            return;
                    }

                    Logger.Verbose(user.Username, $"Setting orientation to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync($"Alright, you are now `{message.Content}`. \n" +
                        $"Next lets set your `fur`.\n" +
                        $"Species, type, color, etc..");
                }
                else if (databaseUser.Fur == null)
                {
                    databaseUser.Fur = message.Content;
                    Logger.Verbose(user.Username, $"Setting fur to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync($"Fur set to `{message.Content}`. Next lets set your `age`.\n" +
                        $"If you don't want to show your age you can just type `no`");
                }
                else if (databaseUser.Age == 0)
                {
                    if (int.TryParse(message.Content, out var age))
                    {
                        databaseUser.Age = age;
                    }
                    else if (message.Content.Equals("no"))
                    {
                        databaseUser.Age = -1;
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("I don't think that's a number. Please only type numbers like `16`.");
                        return;
                    }

                    Logger.Verbose(user.Username, $"Setting age to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync($"Cool, I've set your age to `{message.Content}`. Now comes the fun part!.\n" +
                        $"Describe yourself! What do you like, what do you do, hobbies, favorite color, etc...\n" +
                        $"Type anything you want in one sentence! This will be used as an icebreaker and to get to know you.");
                }
                else if (databaseUser.Description == null)
                {
                    Logger.Verbose(user.Username, $"Setting description to {message.Content}");

                    databaseUser.Description = message.Content;

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync($"Alright, your description will look like: `{message.Content}`." +
                        $"Lastly got a `ref`? Paste dat link.\n" +
                        $"You can just type `none` otherwise.");
                }
                else if (databaseUser.Ref == null)
                {
                    if (Uri.TryCreate(message.Content, UriKind.Absolute, out var uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                    {
                        databaseUser.Ref = message.Content;
                    }
                    else if (message.Content.ToLower().Equals("none"))
                    {
                        databaseUser.Ref = "none";
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("I don't think that is a link or `none`. Try again.");
                        return;
                    }

                    Logger.Verbose(user.Username, $"Setting ref to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync($"How about `RP`? Do you want to be able to see rp rooms? `yes` or `no`\n" +
                        $"This can be changed later on if you change your mind.");
                }
                else if (databaseUser.Monk == false)
                {
                    if (message.Content.ToLower().Equals("no"))
                    {
                        Logger.Verbose(user.Username, $"Disabling RP.");
                        Predicate<SocketRole> monkFinder = (SocketRole sr) => { return sr.Name == "monk"; };
                        SocketRole monk = roles.Find(monkFinder);

                        await user.AddRoleAsync(monk);
                        databaseUser.Monk = true;

                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                        await message.Channel.SendMessageAsync($"I've disabled `RP` for you.\n" +
                            $"Next we are a server with a `NSFW` section. You can opt-in with a `yes` or opt-out with a `no`.\n" +
                            $"This can be changed later on if you change your mind.");
                    }
                    else if (message.Content.ToLower().Equals("yes"))
                    {
                        Logger.Verbose(user.Username, $"Enabled RP.");
                        databaseUser.Monk = true;

                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                        await message.Channel.SendMessageAsync($"I've enabled `RP` for you.\n" +
                            $"Next we are a server with a `NSFW` section. You can opt-in with a `yes` or opt-out with a `no`.\n" +
                            $"This can be changed later on if you change your mind.");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync($"Plese answer with a `yes` or `no`");
                    }
                }
                else if (databaseUser.Nsfw == false)
                {
                    if (message.Content.ToLower().Equals("yes"))
                    {
                        Logger.Verbose(user.Username, $"Enabling NSFW.");
                        databaseUser.Nsfw = true;

                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                        await message.Channel.SendMessageAsync($"I've enabled `NSFW` for you.\n" +
                            $"That's it! Your profile has been set and you are ready to venture into our server.\n" +
                            $"Just type `yes` if you agree to the server rules  and guidelines over at #rules_and_announcements.\n" +
                            $"Take all the time you need, we'll still be here ^^");
                    }
                    else if (message.Content.ToLower().Equals("no"))
                    {
                        Logger.Verbose(user.Username, $"Disabling NSFW.");
                        Predicate<SocketRole> sfwFinder = (SocketRole sr) => { return sr.Name == "SFW"; };
                        SocketRole sfw = roles.Find(sfwFinder);

                        await user.AddRoleAsync(sfw);
                        databaseUser.Nsfw = true;

                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                        await message.Channel.SendMessageAsync($"I've disabled `NSFW` for you.\n" + 
                            $"That's it! Your profile has been set and you are ready to venture into our server.\n" +
                            $"Just type yes if you agree to the server rules  and guidelines over at #rules_and_announcements.\n" +
                            $"Take all the time you need, we'll still be here ^^");
                    }

                    
                }
                else
                {
                    Logger.Verbose(user.Username, $"No more data for user. Checking for agreement: {message.Content}");
                    if(message.Content.ToLower() == "yes" || message.Content.ToLower() == "y")
                    {
                        await message.Channel.SendMessageAsync($"Awesome! Let me create your `room` and set up your permissions...");
                        
                        Predicate<SocketRole> everyoneFinder = (SocketRole sr) => { return sr.Name == "@everyone"; };
                        SocketRole everyone = roles.Find(everyoneFinder);

                        // Creat personal room
                        RestTextChannel introRoom = await guild.CreateTextChannelAsync($"room-{user.Id}");

                        // Make room only visible to new user and admins
                        await introRoom.AddPermissionOverwriteAsync(user, userPerm);
                        await introRoom.AddPermissionOverwriteAsync(everyone, removeAllPerm);

                        Thread.Sleep(500);
                        await message.Channel.SendMessageAsync($"Adding sparkles...");
                        Thread.Sleep(700);
                        await message.Channel.SendMessageAsync($"Done!");
                        Thread.Sleep(300);
                        await message.Channel.SendMessageAsync($"I've created a `room` for you over at: #room-{user.Id}. " +
                            $"You can always type `!help` for any issues or talk with the staff, most of us don't bite :)");
                        Thread.Sleep(1000);

                        Predicate<SocketRole> newbieFinder = (SocketRole sr) => { return sr.Name == "Newbie"; };
                        SocketRole newbie = roles.Find(newbieFinder);

                        await user.RemoveRoleAsync(newbie);
                        databaseUser.TutorialFinished = true;

                        await lobby.SendMessageAsync($"Please welcome <@{user.Id}> to the server!");

                        await message.Channel.SendMessageAsync("This channel will self-destruct in 10 seconds");

                        Thread.Sleep(10000);

                        await (message.Channel as SocketTextChannel).DeleteAsync();
                        
                    }
                    else if (message.Content.ToLower() == "no" || message.Content.ToLower() == "n")
                    {
                        await message.Channel.SendMessageAsync($"That's sad to hear...\n" +
                            $"We'll still be here when you are ready.");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync($"Please only answer `yes` or `no`");
                    }

                }

                db.Users.Attach(databaseUser);
                db.SaveChanges();
            }
            return;
        }

    }
}
