using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Commands;
using LunaBot.Database;
using LunaBot.ServerUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        private readonly DiscordSocketClient _client;
        private CommandService _commands;
        private CommandService _setAttributes;
        private CommandService _getAttributes;
        private IServiceProvider _services;

        public SocketGuild guild;
        public SocketTextChannel lobby;
        public List<SocketRole> roles;

        public Engine()
        {
            this._client = new DiscordSocketClient();
            this._commands = new CommandService();
            this._setAttributes = new CommandService();
            this._getAttributes = new CommandService();

            this.commandDictionary = new Dictionary<string, BaseCommand>();
            this.aliasDictionary = new Dictionary<string, string>();
            this.messageTimestamps = new Dictionary<ulong, DateTime>();
        }

        public async Task RunAsync()
        {
            _client.Log += Logger.LogAsync;

            string token = SecretStrings.token;
            
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_setAttributes)
                .AddSingleton(_getAttributes)
                .BuildServiceProvider();

            await InstallCommandsAsync();
            
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            
            //_client.MessageReceived += MessageReceivedAsync;
            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
            _client.UserBanned += UserBannedAsync;
            _client.MessageDeleted += MessageDeletedAsync;

            //this.RegisterCommands();

            _client.Ready += ReadyAsync;

            await Task.Delay(-1);
        }

        private async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandsAsync;

            await _commands.AddModuleAsync<Modules.CommandsUser>();
            await _commands.AddModuleAsync<Modules.CommandsMod>();
            await _commands.AddModuleAsync<Modules.CommandsAdmin>();
            await _commands.AddModuleAsync<Modules.CommandsOwner>();

            await _setAttributes.AddModuleAsync<Modules.SetAttributes>();
            await _getAttributes.AddModuleAsync<Modules.GetAttributes>();
        }

        private async Task HandleCommandsAsync(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;

            if (message == null)
                return;

            await message.LogAsync(LogSeverity.Verbose).ConfigureAwait(false);

            if (await ProcessMessageAsync(message))
                return;

            int argPos = 1;
            string messageText = message.Content;
            SocketCommandContext context;
            IResult result;

            try
            {
                // Tutorial messages that cannot run commands.
                if (message.Channel.Name.Contains("intro"))
                {
                    await ProcessTutorialMessaageAsync(message).ConfigureAwait(false);
                    return;
                }
                else if (messageText.StartsWith("!"))
                {
                    context = new SocketCommandContext(_client, message);
                    result = await _commands.ExecuteAsync(context, argPos, _services);
                }
                else if (messageText.StartsWith("+"))
                {
                    context = new SocketCommandContext(_client, message);
                    result = await _setAttributes.ExecuteAsync(context, argPos, _services);
                }
                else if (messageText.StartsWith("?"))
                {
                    context = new SocketCommandContext(_client, message);
                    result = await _getAttributes.ExecuteAsync(context, argPos, _services);
                }
                else
                {
                    await ProcessXpAsync(message);

                    return;
                }

                if (!result.IsSuccess)
                    Logger.Warning("CommandHandler", result.ErrorReason);

            }
            catch(Exception e)
            {
                await BotReporting.ReportAsync(ReportColors.exception, 
                    (SocketTextChannel)message.Channel, 
                    "Exception encountered while running a command, contact administrator.", 
                    e.Message + ":\n" + e.InnerException, 
                    luna);
            }

        }
                
        private async Task ReadyAsync()
        {
            guild = _client.GetGuild(Guilds.Guild);
            await guild.DownloadUsersAsync();
            lobby = _client.GetChannel(Channels.Lobby) as SocketTextChannel;
            roles = guild.Roles.ToList();
            BotReporting.SetBotReportingChannel(guild.GetTextChannel(Channels.BotLogs));
            luna = guild.GetUser(UserIds.Luna);

            // Adding owners
            using (DiscordContext db = new DiscordContext())
            {
                db.Database.EnsureCreated();
                foreach (ulong userId in UserIds.Owners)
                {
                    User owner = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();
                    if (owner == null)
                    {
                        Logger.Warning("System", "An owner not found in the database, adding as Owner.");

                        User newUser = new User();
                        newUser.DiscordId = userId;
                        newUser.Level = 1;
                        newUser.Privilege = User.Privileges.Owner;
                        newUser.TutorialFinished = true;

                        db.Users.Add(newUser);
                        db.SaveChanges();

                        Logger.Verbose("System", "Created Owner: " + newUser.DiscordId);
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
            await _client.SetGameAsync("!help");

            await LobbyAnnouncements.StartupConfirmationAsync(lobby);

            // Remove all mute from muted users
            
        }

        private async Task UserJoinedAsync(SocketUser user)
        {
            Logger.Info("System", $"User {user.Username}<{user.Id}> joined the server.");
            if (lobby == null)
                lobby = _client.GetChannel(343193171431522304) as SocketTextChannel;

            RegisterCommand registerCommand = new RegisterCommand();

            registerCommand.manualRegister(user as SocketGuildUser);
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = user.Id;
                User databaseUser = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();
                if (databaseUser.TutorialFinished)
                {
                    await ReestablishUserPreferences(databaseUser, (SocketGuildUser)user);
                    Logger.Verbose("System", $"{user.Username}<@{user.Id}> already finished the tutorial. Announcing in lobby.");
                    await lobby.SendMessageAsync($"Welcome <@{user.Id}> back to the server!");
                }
                else
                {
                    Logger.Verbose("System", $"Placing {user.Username}<@{user.Id}> through tutorial...");
                    await StartTutorialAsync(user as SocketGuildUser).ConfigureAwait(false);

                }
            }

            await BotReporting.ReportAsync(ReportColors.userJoined,
                        channel : null,
                        title : "User Joined",
                        content : $"<@{user.Id}> has joined the server.",
                        originUser : luna,
                        targetUser : user).ConfigureAwait(false);

        }

        private async Task UserLeftAsync(SocketUser user)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = user.Id;
                User u = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();
                if (u.TutorialFinished)
                {
                    Logger.Info("System", $"User {user.Username}<@{user.Id}> has left the server.");
                    await lobby.SendMessageAsync($"{user.Username} has left the server :wave:").ConfigureAwait(false);
                }
            }

            await BotReporting.ReportAsync(ReportColors.userLeft,
                       channel: null,
                       title: "User Left",
                       content: $"<@{user.Id}> has left the server.",
                       originUser: luna,
                       targetUser: user).ConfigureAwait(false);
        }

        private async Task UserBannedAsync(SocketUser user, SocketGuild guild)
        {
            await lobby.SendMessageAsync($"My hammer to your face!");
            Logger.Info("System", $"User {user.Username}<@{user.Id}> has been banned from the server.");

            await BotReporting.ReportAsync(ReportColors.userBanned,
                       channel: null,
                       title: "User Banned",
                       content: $"<@{user.Id}> has been banned.",
                       originUser: luna,
                       targetUser: user).ConfigureAwait(false);
        }

        /// <summary>
        /// Reports deleted messages in the BotReporting channel
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        private async Task MessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            Logger.Info("System", $"User {message.Value.Author.Username} <@{message.Value.Id}> deleted the message: {message.Value.Content}.");

            SocketUser user = (SocketUser)message.Value.Author;
            SocketTextChannel textChannel = (SocketTextChannel)channel;

            await BotReporting.ReportAsync(ReportColors.userCommand,
                       channel: textChannel,
                       title: $"{message.Value.Author.Username} deleted message",
                       content: $"<@{message.Value.Id}> deleted a message in {channel.Name}:\n" +
                                $"{message.Value.Content}",
                       originUser: user,
                       targetUser: user).ConfigureAwait(false);
        }
        
        private async Task ProcessXpAsync(SocketMessage message)
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
                        await message.Channel.SendMessageAsync($"Congrats <@{user.DiscordId}>! You leveled up to {user.Level}! :confetti_ball:");
                    }
                }

                // Updates last time message was recieved.
                user.LastMessage = DateTime.UtcNow;

                db.Users.Attach(user);
                db.SaveChanges();
            }
        }
        
        /// <summary>
        /// Processes messages and prevents raids by checking the newest message sent by the user and deletes if it doesn't pass criteria
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if deleted, false otherwise.</returns>
        private async Task<bool> ProcessMessageAsync(SocketMessage message)
        {
            ulong user = message.Author.Id;
            DateTime userTimestamp = message.Timestamp.DateTime;
            DateTime cachedTimestamp;

            // Ignore Luna
            if (message.Author.Id == UserIds.Luna)
                return false;
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                User databaseUser = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();
                if (databaseUser.Privilege >= User.Privileges.Moderator)
                    return false;
            }

            if(messageTimestamps.TryGetValue(user, out cachedTimestamp))
            {
                if (userTimestamp.Subtract(cachedTimestamp) < TimeSpan.FromSeconds(1))
                {
                    Logger.Info("System", $"User {message.Author.Username}<{message.Author.Id}> is talking too fast. Deleting latest message.");
                    await message.DeleteAsync();

                    await message.Channel.SendMessageAsync($"<@{message.Author.Id}> you are talking too fast, please slow down.");

                    Task.Run(async () =>
                    {
                        await BotReporting.ReportAsync(ReportColors.spamBlock, 
                            (SocketTextChannel)message.Channel, 
                            "User talking too fast.", 
                            $"<@{message.Author.Id}>: {message.Content}", 
                            luna, 
                            message.Author);

                        // add warn
                    }).Start();

                    return true;
                }
                else
                {
                    messageTimestamps[user] = userTimestamp;
                    return false;
                }
            }

            messageTimestamps.Add(user, DateTime.Now);

            return false;
        }

        /// <summary>
        /// Sets newbie role to user and places them through the tutorial.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<bool> StartTutorialAsync(SocketGuildUser user)
        {
            Predicate<SocketRole> newbieFinder = (SocketRole sr) => { return sr.Name == "Newbie"; };
            Predicate<SocketRole> everyoneFinder = (SocketRole sr) => { return sr.Name == "@everyone"; };
            SocketRole newbie = roles.Find(newbieFinder);
            SocketRole everyone = roles.Find(everyoneFinder);

            await user.AddRoleAsync(newbie);

            // Creat intro room
            RestTextChannel introRoom = await guild.CreateTextChannelAsync($"intro-{user.Id}");

            // Make room only visible to new user and admins
            await introRoom.AddPermissionOverwriteAsync(user, Permissions.userPerm);
            //await introRoom.AddPermissionOverwriteAsync(everyone, Permissions.removeAllPerm);
            //await introRoom.AddPermissionOverwriteAsync(luna, Permissions.adminPerm);
            //await introRoom.AddPermissionOverwriteAsync(guild.GetRole(411752657863049228), Permissions.adminPerm);

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

        private async Task ReestablishUserPreferences(User databaseUser, SocketGuildUser user)
        {
            // Set Gender
            Predicate<SocketRole> genderFinder;
            SocketRole gender;

            genderFinder = (SocketRole sr) => { return sr.Name == databaseUser.Gender.ToString().ToLower(); };
            gender = roles.Find(genderFinder);
            if (gender != null)
            {
                await user.AddRoleAsync(gender);
                Logger.Info(user.Username, $"Changed user <@{user.Id}>'s gender to {databaseUser.Gender}");
            }
            else
            {
                Logger.Error("System", $"Could not find user gender {databaseUser.Gender.ToString().ToString()}");
            }

            // Set Orientation
            Predicate<SocketRole> orientationFinder;
            SocketRole orientation;

            // Remove old role
            orientationFinder = (SocketRole sr) => { return sr.Name == databaseUser.orientation.ToString().ToLower(); };
            orientation = roles.Find(orientationFinder);
            if (orientation != null)
            {
                await user.AddRoleAsync(orientation);
                Logger.Info(user.Username, $"Changed user <@{user.Id}>'s orientation to {databaseUser.orientation.ToString()}");
            }
            else
            {
                Logger.Error("System", $"Could not find user orientation {databaseUser.orientation.ToString().ToString()}");
            }


        }

        private async Task ProcessTutorialMessaageAsync(SocketMessage message)
        {
            // Ignore Luna
            // Ignore tutorial finished people -
            // Find what step they are on
            // Order is Nickname, Gender, Age, Fur,Description, Ref
            // process response
            //  -> ask next question and fill in data
            // !-> ask to provide a correct answer
            if (message.Author == luna)
                return;

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

                if (databaseUser.Nickname == null || databaseUser.Nickname == "")
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
                            databaseUser.Gender = User.Genders.TransMale;
                            break;
                        case "trans-female":
                            genderFinder = (SocketRole sr) => { return sr.Name == Roles.TransFemale; };
                            gender = roles.Find(genderFinder);
                            await user.AddRoleAsync(gender);
                            databaseUser.Gender = User.Genders.TransFemale;
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
                            orientationFinder = (SocketRole sr) => { return sr.Name == Roles.Straight; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Straight;
                            break;
                        case "gay":
                        case "g":
                            orientationFinder = (SocketRole sr) => { return sr.Name == Roles.Gay; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Gay;
                            break;
                        case "bisexual":
                        case "bi":
                            orientationFinder = (SocketRole sr) => { return sr.Name == Roles.Bi; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Bi;
                            break;
                        case "asexual":
                            orientationFinder = (SocketRole sr) => { return sr.Name == Roles.Asexual; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Asexual;
                            break;
                        case "gray-a":
                            orientationFinder = (SocketRole sr) => { return sr.Name == Roles.GrayA; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Gray;
                            break;
                        case "pansexual":
                        case "pan":
                            orientationFinder = (SocketRole sr) => { return sr.Name == Roles.Pan; };
                            orientation = roles.Find(orientationFinder);
                            await user.AddRoleAsync(orientation);
                            databaseUser.orientation = User.Orientation.Pan;
                            break;
                        default:
                            await message.Channel.SendMessageAsync("Hmm... That's not an orientation I can undestand.\n" +
                                "Make sure it's either straight, gay, bisexaul, asexual, or gray-a.");
                            return;
                    }

                    Logger.Verbose(user.Username, $"Setting orientation to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync($"Alright, you are now `{message.Content}`. \n" +
                        $"Next lets set your `sona`.\n" +
                        $"Species, type, color, etc..");
                }
                else if (databaseUser.Fur == null)
                {
                    databaseUser.Fur = message.Content;
                    Logger.Verbose(user.Username, $"Setting sona to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync($"Sona set to `{message.Content}`. Next lets set your `age`.\n" +
                        $"If you don't want to show your age you can just type `no`");
                }
                else if (databaseUser.Age == 0)
                {
                    if (int.TryParse(message.Content, out var age))
                    {
                        if(age < 0)
                        {
                            await message.Channel.SendMessageAsync("Please only use positive numbers.");
                            return;
                        }

                        databaseUser.Age = age;
                    }
                    else if (message.Content.ToLower().Equals("no"))
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

                    await message.Channel.GetMessagesAsync(10).ForEachAsync((x) => { foreach (var f in x) {
                            f.DeleteAsync();
                            Thread.Sleep(50);
                        } });
                    await message.Channel.SendMessageAsync($"How about `RP`? Do you want to be able to see rp rooms? `yes` or `no`\n" +
                        $"This can be changed later on if you change your mind.");
                }
                else if (databaseUser.Monk == false)
                {
                    if (message.Content.ToLower().Equals("no"))
                    {
                        Logger.Verbose(user.Username, $"Disabling RP.");
                        Predicate<SocketRole> monkFinder = (SocketRole sr) => { return sr.Name == Roles.Monk; };
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
                        Predicate<SocketRole> sfwFinder = (SocketRole sr) => { return sr.Name == Roles.SFW; };
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
                        databaseUser.TutorialFinished = true;
                        db.SaveChanges();

                        await (message.Channel as SocketGuildChannel).AddPermissionOverwriteAsync(user, Permissions.mutePerm);

                        await message.Channel.SendMessageAsync($"Awesome! Let me create your `room` and set up your permissions...");
                        
                        Predicate<SocketRole> everyoneFinder = (SocketRole sr) => { return sr.Name == "@everyone"; };
                        SocketRole everyone = roles.Find(everyoneFinder);

                        // Creat personal room
                        RestTextChannel personalRoom = await guild.CreateTextChannelAsync($"room-{user.Id}");

                        // Make room only visible to new user and admins
                        await personalRoom.AddPermissionOverwriteAsync(user, Permissions.roomPerm);
                        await personalRoom.AddPermissionOverwriteAsync(everyone, Permissions.removeAllPerm);

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

                        await lobby.SendMessageAsync($"Please welcome <@{user.Id}> to the server!");

                        await message.Channel.SendMessageAsync("This channel will self-destruct in 2 minutes");

                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;

                            Thread.Sleep(120000);
                            (message.Channel as SocketTextChannel).DeleteAsync();

                        }).Start();
                        
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
                
                db.SaveChanges();
            }
            return;
        }

    }
}
