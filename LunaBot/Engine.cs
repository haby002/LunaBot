using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Database;
using LunaBot.ServerUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace LunaBot
{
    class Engine
    {
        private IDictionary<string, string> aliasDictionary;
        private IDictionary<ulong, DateTime> messageTimestamps;

        
        public static SocketGuildUser luna;

        private readonly DiscordSocketClient _client;
        private CommandService _commands;
        private CommandService _setAttributes;
        private CommandService _getAttributes;
        private IServiceProvider _services;
        IReadOnlyCollection<RestInviteMetadata> _invites;

        public SocketGuild guild;
        public SocketTextChannel lobby;
        public List<SocketRole> roles;

        public Engine()
        {
            this._client = new DiscordSocketClient();
            this._commands = new CommandService();
            this._setAttributes = new CommandService();
            this._getAttributes = new CommandService();
            
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
            
            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
            _client.UserBanned += UserBannedAsync;
            _client.MessageDeleted += MessageDeletedAsync;

            _invites = (await guild.GetInvitesAsync());

            _client.Ready += ReadyAsync;

            await Task.Delay(-1);
        }

        private async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandsAsync;
            
            await _commands.AddModuleAsync<Modules.CommandsUser>(_services);
            await _commands.AddModuleAsync<Modules.CommandsMod>(_services);
            await _commands.AddModuleAsync<Modules.CommandsAdmin>(_services);
            await _commands.AddModuleAsync<Modules.CommandsOwner>(_services);

            await _setAttributes.AddModuleAsync<Modules.SetAttributes>(_services);
            await _getAttributes.AddModuleAsync<Modules.GetAttributes>(_services);
        }

        private async Task HandleCommandsAsync(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;

            if (message == null || message.Author.IsBot || message.Author.IsWebhook)
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
                if (messageText.StartsWith("!"))
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
                else if (message.Channel.Name.Contains("intro"))
                {
                    await ProcessTutorialMessaageAsync(message).ConfigureAwait(false);
                    return;
                }
                else
                {
                    await ProcessXpAsync(message);

                    await SecretFeaturesAsync(message);

                    return;
                }

                if (!result.IsSuccess)
                    Logger.Warning("CommandHandler", result.ErrorReason);

            }
            catch(Exception e)
            {
                await BotReporting.ReportAsync(ReportColors.exception, 
                    (SocketTextChannel)message.Channel,
                    e.Message,
                    $"Source: {e.Source}\n {e.InnerException}\n {e.StackTrace}", 
                    message.Author);
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

            UserUtilities.manualRegister(user as SocketGuildUser);

            // Log user joined and announce
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = user.Id;
                User databaseUser = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();
                if (databaseUser.TutorialFinished)
                {
                    await ReestablishUserPreferencesAsync(databaseUser, (SocketGuildUser)user);
                    Logger.Verbose("System", $"{user.Username}<@{user.Id}> already finished the tutorial. Announcing in lobby.");
                    await lobby.SendMessageAsync($"Welcome <@{user.Id}> back to the server!");
                    await lobby.SendMessageAsync("I cannot set SFW or RP roles. Please set these using `+sfw <yes, no>` and `+monk <yes, no>`");
                }
                else
                {
                    Logger.Verbose("System", $"Placing {user.Username}<@{user.Id}> through tutorial...");
                    await StartTutorialAsync(user as SocketGuildUser).ConfigureAwait(false);

                }
            }

            // Check what invite the user used
            IReadOnlyCollection<RestInviteMetadata> newInvites = await guild.GetInvitesAsync();

            Dictionary<string, int?> newInvitesDict = newInvites.ToDictionary(i => i.Code, i => i.Uses);
            RestInviteMetadata invite = _invites.FirstOrDefault(i => newInvitesDict[i.Code] != i.Uses);
            
            // Report user joined
            await BotReporting.ReportAsync(ReportColors.userJoined,
                        channel : null,
                        title : "User Joined",
                        content : $"<@{user.Id}> {user.Username} has joined the server using invite {invite.Code} from {invite.Inviter.Username}",
                        originUser : luna,
                        targetUser : user).ConfigureAwait(false);

        }

        private async Task UserLeftAsync(SocketUser user)
        {
            SocketTextChannel channel = null;

            Logger.Info("System", $"User {user.Username}<@{user.Id}> has left the server.");
            await BotReporting.ReportAsync(ReportColors.userLeft,
                       channel: null,
                       title: "User Left",
                       content: $"<@{user.Id}> {user.Username} has left the server.",
                       originUser: luna,
                       targetUser: user).ConfigureAwait(false);

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = user.Id;
                User u = db.Users.Where(x => x.DiscordId == userId).FirstOrDefault();

                if (u.TutorialFinished)
                {
                    await lobby.SendMessageAsync($"{user.Username} has left the server :wave:").ConfigureAwait(false);

                    channel = guild.TextChannels.Where(x => x.Name == $"room-{user.Id}").FirstOrDefault();
                }
                else
                {
                    channel = guild.TextChannels.Where(x => x.Name == $"intro-{user.Id}").FirstOrDefault();
                }

            }

            if (channel != null)
            {
                await channel.DeleteAsync();

                await BotReporting.ReportAsync(ReportColors.userLeft,
                      channel: null,
                      title: "Room deleted",
                      content: $"{user.Username} has left the server, room {channel.Name} deleted.",
                      originUser: luna,
                      targetUser: user).ConfigureAwait(false);
            }
        }

        private async Task UserBannedAsync(SocketUser user, SocketGuild guild)
        {
            // Ignore users with no profile or haven't finished the tutorial.
            using(DiscordContext db = new DiscordContext())
            {
                User databaseUser = db.Users.Where((u) => user.Id == u.DiscordId).First();

                if (databaseUser == null || databaseUser.TutorialFinished == false)
                {
                    return;
                }
            }

            await lobby.SendMessageAsync($"My hammer to your face {user.Username}!");
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
                       originUser: user).ConfigureAwait(false);
        }
        
        private async Task ProcessXpAsync(SocketMessage message)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                User user = db.Users.Where(x => x.DiscordId == userId).SingleOrDefault();
                SocketGuildUser discordUser = message.Author as SocketGuildUser;

                // No XP gain if you only say 2 words or less.
                int words = (message.Content.Split(' ').Count<string>());

                if (words < 3)
                    return;

                int xp = message.Content.Split(' ').Select(x => x.Trim()).Where((s) => s.Count() > 2).Sum(x => x.Count());
                // Adds characters (no whitespace) as XP. Returns true if user leveled up.
                if (user.AddXP(xp))
                {
                    string reply = "";
                    
                    if (user.Level % 10 == 0)
                    {
                        await message.Channel.SendMessageAsync($"Congrats <@{user.DiscordId}>! You leveled up to {user.Level}! :confetti_ball:");
                    }

                    SocketRole verifiedrole = guild.Roles.Where(r => r.Name == Roles.Verified).FirstOrDefault();

                    if (user.Level >= 10 && !discordUser.Roles.Contains(verifiedrole))
                    {
                        await discordUser.AddRoleAsync(verifiedrole);
                        reply += $"You now have the verified role and can add reactions!";
                    }
                    
                    if(reply != "")
                        await (await message.Channel.SendMessageAsync(reply)).AddReactionAsync(new Emoji("😸"));
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

                // Check for banned words
                foreach (string bannedWord in BannedWords.words)
                {
                    if (Regex.IsMatch(message.Content, @"\b" + bannedWord + @"\b"))
                    {
                        await message.DeleteAsync();

                        await message.Author.SendMessageAsync("One or more of the words in your latest message is a banned word.\n" +
                            "You have been warned and when you get 5 warns you will be kicked.\n");

                        await BotReporting.ReportAsync(ReportColors.spamBlock,
                                (SocketTextChannel)message.Channel,
                                $"User said banned word. Warn given and total is {++databaseUser.warnCount}",
                                $"<@{message.Author.Id}>: {message.Content}",
                                luna,
                                message.Author);

                        if(databaseUser.warnCount >= 5)
                        {
                            await KickUserHelper.KickAsync(message.Channel as SocketTextChannel, message.Author as SocketGuildUser);
                        }

                        db.SaveChanges();

                        return true;
                    }
                }

                // Return if user is mod or higher
                if (databaseUser.Privilege >= User.Privileges.Moderator)
                    return false;

                // Return if user is above level 5
                if (databaseUser.Level >= 5)
                    return false;
            }

            if(messageTimestamps.TryGetValue(user, out cachedTimestamp))
            {
                if (userTimestamp.Subtract(cachedTimestamp) < TimeSpan.FromSeconds(1))
                {
                    Logger.Info("System", $"User {message.Author.Username}<{message.Author.Id}> is talking too fast. Deleting latest message.");
                    await message.DeleteAsync();

                    RestUserMessage r = await message.Channel.SendMessageAsync($"<@{message.Author.Id}> you are talking too fast, please slow down.");

                    await BotReporting.ReportAsync(ReportColors.spamBlock,
                            (SocketTextChannel)message.Channel,
                            "User talking too fast.",
                            $"<@{message.Author.Id}>: {message.Content}",
                            luna,
                            message.Author);

                    Task.Run(async () =>
                    {
                        Thread.Sleep(7000);
                        await r.DeleteAsync();
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
        /// Secret Features for Secret stuff
        /// </summary>
        /// <returns></returns>
        private async Task SecretFeaturesAsync(SocketUserMessage message)
        {
            Random r = new Random();
            
            Match match = Regex.Match(message.Content.ToLower(), @"\ba+w+oo+\b");
            if (match.Success)
            {
                // await BotReporting.ReportAsync(ReportColors.modCommand, message.Channel as SocketTextChannel, "Secret activated!", $"Activating message: {message.Content}", luna);

                if (r.Next(15) != 4)
                    return;
                
                await message.Channel.SendMessageAsync("No awoo'ing allowed! You have been fined $" + match.Length * 12.50 + ".\n" +
                    "*We also accept dog biscuits.*");
            }
            else if(Regex.IsMatch(message.Content.ToLower(), @"\bbulge\b"))
            {
                if (r.Next(15) != 4)
                    return;
                
                await message.Channel.SendMessageAsync("OWO");
            }
            
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

            // Register user in database
            bool register = UserUtilities.manualRegister(user);

            // Start interaction with user. Sleeps are for humanizing the bot.
            await introRoom.SendMessageAsync("(1/10)");
            await introRoom.SendMessageAsync("Welcome to the server! Lets get you settled, alright?");
            Thread.Sleep(2000);
            await introRoom.SendMessageAsync("Firstly, what should we call you? \n" +
                "If you'd rather not change your name just type `none`");

            return register;
            //await introRoom.DeleteAsync();

            //RestTextChannel personalRoom = await guild.CreateTextChannelAsync($"room-{user.Id}");
            //await personalRoom.SendMessageAsync("This is your room, you can do whatever you want in here. Try using the !help command to start ;)");
            
        }

        private async Task ReestablishUserPreferencesAsync(User databaseUser, SocketGuildUser user)
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
                Task awaitableObject;

                if (databaseUser.TutorialFinished)
                {
                    Logger.Verbose("System", "Registered user talking in tutorial room.");
                    
                    return;
                }

                if (databaseUser.Nickname == null || databaseUser.Nickname == "")
                {
                    if(message.Content.ToLower().Equals("none"))
                    {
                        databaseUser.Nickname = message.Author.Username;
                        
                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) {awaitableObject = f.DeleteAsync(); } });

                        await message.Channel.SendMessageAsync("(2/10)");
                        await message.Channel.SendMessageAsync("Very well, your name will not be changed. Now lets set your gender.");
                    }
                    else
                    {
                        SocketGuildUser guildUser = message.Author as SocketGuildUser;
                        await guildUser.ModifyAsync(n => n.Nickname = message.Content);
                        databaseUser.Nickname = message.Content;
                        
                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                        
                        Logger.Verbose(user.Username, $"Changed nickname from {user.Username} to {message.Content}");
                        await message.Channel.SendMessageAsync("(2/10)");
                        await message.Channel.SendMessageAsync("I've gone ahead and changed your name. Now lets set your gender.");
                    }
                    
                    await message.Channel.SendMessageAsync("You can choose between:\n" +
                        "- Male\n" +
                        "- Female\n" +
                        "- Trans-Female\n" +
                        "- Trans-Male\n" +
                        "- None \n" +
                        "- Fluid \n" +
                        "- Other");
                }
                else if (databaseUser.Gender == User.Genders.Null)
                {
                    
                    User.Genders gender = EnumParsers.StringToGender(message.Content);

                    if(gender == User.Genders.Null)
                    {
                        await message.Channel.SendMessageAsync("I'm sorry I couldn't understand your message. Make sure it's either `male`, `female`, `trans-male`, `trans-female`, or `other`.");
                        return;
                    }

                    Predicate<SocketRole> genderFinder = (SocketRole sr) => { return sr.Name == gender.ToString().ToLower(); };
                    SocketRole genderRole = roles.Find(genderFinder);
                    await user.AddRoleAsync(genderRole);
                    databaseUser.Gender = gender;

                    Logger.Verbose(user.Username, $"Setting gender to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync("(3/10)");
                    await message.Channel.SendMessageAsync($"Alright, you are now `{message.Content}`. \n" +
                        $"Next lets set your orientation.\n" +
                        "You can choose from below:\n" +
                        "- Straight\n" +
                        "- Gay\n" +
                        "- Bisexual\n" +
                        "- Asexual\n" +
                        "- Pansexual\n" +
                        "- Demisexual\n" +
                        "- other");
                }
                else if(databaseUser.orientation == User.Orientation.None)
                {
                    User.Orientation orientation = EnumParsers.StringToOrientation(message.Content);

                    if (orientation == User.Orientation.None)
                    {
                        await message.Channel.SendMessageAsync("Hmm... That's not an orientation I can undestand.\n" +
                                                        "Make sure it's either straight, gay, bisexaul, asexual, or gray-a.");
                        return;
                    }

                    Predicate<SocketRole> orientationFinder = (SocketRole sr) => { return sr.Name == orientation.ToString().ToLower(); };
                    SocketRole orientationRole = roles.Find(orientationFinder);
                    await user.AddRoleAsync(orientationRole);
                    databaseUser.orientation = orientation;

                    Logger.Verbose(user.Username, $"Setting gender to {message.Content}");

                    Logger.Verbose(user.Username, $"Setting orientation to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync("(4/10)");
                    await message.Channel.SendMessageAsync($"Alright, you are now `{message.Content}`. \n" +
                        $"Next lets set your `sona`.\n" +
                        $"Species, type, color, etc..");
                }
                else if (databaseUser.Fur == null)
                {
                    databaseUser.Fur = message.Content;
                    Logger.Verbose(user.Username, $"Setting sona to {message.Content}");

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync("(5/10)");
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
                        else if(age > 100)
                        {
                            await message.Channel.SendMessageAsync("You don't expect me to believe that you are over 80 years old right?");
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

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync("(6/10)");
                    await message.Channel.SendMessageAsync($"Cool, I've set your age to `{message.Content}`. Now comes the fun part!.\n" +
                        $"Describe yourself! What do you like, what do you do, hobbies, favorite color, etc...\n" +
                        $"Type anything you want in one sentence! This will be used as an icebreaker and to get to know you.");
                }
                else if (databaseUser.Description == null)
                {
                    Logger.Verbose(user.Username, $"Setting description to {message.Content}");

                    databaseUser.Description = message.Content;

                    await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                    await message.Channel.SendMessageAsync("(7/10)");
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
                            awaitableObject = f.DeleteAsync();
                            Thread.Sleep(50);
                        } });
                    await message.Channel.SendMessageAsync("(8/10)");
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

                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                        if(databaseUser.Age < 18)
                        {
                            Logger.Verbose(user.Username, $"Skipping NSFW due to age.");
                            Logger.Verbose(user.Username, $"Disabling NSFW.");
                            Predicate<SocketRole> sfwFinder = (SocketRole sr) => { return sr.Name == Roles.SFW; };
                            SocketRole sfw = roles.Find(sfwFinder);

                            await user.AddRoleAsync(sfw);
                            databaseUser.Nsfw = true;

                            await message.Channel.SendMessageAsync("(10/10)");
                            await message.Channel.SendMessageAsync($"I've disabled `RP` for you.\n" +
                                $"That's it! Your profile has been set and you are ready to venture into our server.\n" +
                                $"Just type `yes` if you agree to the server rules  and guidelines over at #rules.\n" +
                                $"Additionally we have roles for free games or updates to our main bot, check out the pins in the bot corner for the commands.\n" +
                                $"Take all the time you need, we'll still be here ^^");
                        } 
                        else
                        {
                            await message.Channel.SendMessageAsync("(9/10)");
                            await message.Channel.SendMessageAsync($"I've disabled `RP` for you.\n" +
                                $"Next we are a server with a `NSFW` section. You can opt-in with a `yes` or opt-out with a `no`.\n" +
                                $"This can be changed later on if you change your mind.");
                        }
                    }
                    else if (message.Content.ToLower().Equals("yes"))
                    {
                        Logger.Verbose(user.Username, $"Enabling RP.");
                        databaseUser.Monk = true;

                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                        if(databaseUser.Age < 18)
                        {
                            Logger.Verbose(user.Username, $"Skipping NSFW due to age.");
                            Logger.Verbose(user.Username, $"Disabling NSFW.");
                            Predicate<SocketRole> sfwFinder = (SocketRole sr) => { return sr.Name == Roles.SFW; };
                            SocketRole sfw = roles.Find(sfwFinder);

                            await user.AddRoleAsync(sfw);
                            databaseUser.Nsfw = true;
                            await message.Channel.SendMessageAsync($"I've enabled `RP` for you.\n" +
                                $"That's it! Your profile has been set and you are ready to venture into our server.\n" +
                                $"Just type `yes` if you agree to the server rules  and guidelines over at #rules.\n" +
                                $"Additionally we have roles for free games or updates to our main bot, check out the pins in the bot corner for the commands.\n" +
                                $"Take all the time you need, we'll still be here ^^");
                        } 
                        else
                        {
                            await message.Channel.SendMessageAsync($"I've enabled `RP` for you.\n" +
                                $"Next we are a server with a `NSFW` section. You can opt-in with a `yes` or opt-out with a `no`.\n" +
                                $"This can be changed later on if you change your mind.");
                        }
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

                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                        await message.Channel.SendMessageAsync("(10/10)");
                        await message.Channel.SendMessageAsync($"I've enabled `NSFW` for you.\n" +
                            $"That's it! Your profile has been set and you are ready to venture into our server.\n" +
                            $"Just type `yes` if you agree to the server rules  and guidelines over at #rules.\n" +
                            $"Additionally we have roles for free games or updates to our main bot, check out the pins in the bot corner for the commands.\n" +
                            $"Take all the time you need, we'll still be here ^^");
                    }
                    else if (message.Content.ToLower().Equals("no"))
                    {
                        Logger.Verbose(user.Username, $"Disabling NSFW.");
                        Predicate<SocketRole> sfwFinder = (SocketRole sr) => { return sr.Name == Roles.SFW; };
                        SocketRole sfw = roles.Find(sfwFinder);

                        await user.AddRoleAsync(sfw);
                        databaseUser.Nsfw = true;

                        await message.Channel.GetMessagesAsync().ForEachAsync((x) => { foreach (var f in x) { awaitableObject = f.DeleteAsync(); } });
                        await message.Channel.SendMessageAsync("(10/10)");
                        await message.Channel.SendMessageAsync($"I've disabled `NSFW` for you.\n" + 
                            $"That's it! Your profile has been set and you are ready to venture into our server.\n" +
                            $"Just type yes if you agree to the server rules  and guidelines over at #rules_and_announcements.\n" +
                            $"Additionally we have roles for free games, updates to our main bot, and other customization roles, check out the pins in the bot corner for the commands.\n" +
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

                        await BotReporting.ReportAsync(ReportColors.userCommand,
                            message.Channel as SocketTextChannel,
                            $"<@{user.Username}> tutorial finished",
                            $"Nick: {databaseUser.Nickname}\n" +
                            $"Fur: {databaseUser.Fur}\n" +
                            $"Age: {databaseUser.Age}\n" +
                            $"Description: {databaseUser.Description}\n" +
                            $"Ref: {databaseUser.Ref}\n" +
                            $"Gender: {databaseUser.Gender.ToString()}\n" +
                            $"Monk: {databaseUser.Monk}\n" +
                            $"SFW: {databaseUser.Nsfw}",
                            luna,
                            user,
                            $"ID: {user.Id}");

                        await (message.Channel as SocketGuildChannel).AddPermissionOverwriteAsync(user, Permissions.mutePerm);

                        await message.Channel.SendMessageAsync($"Awesome! Let me create your `room` and set up your permissions...");
                        
                        Predicate<SocketRole> everyoneFinder = (SocketRole sr) => { return sr.Name == "@everyone"; };
                        SocketRole everyone = roles.Find(everyoneFinder);

                        // Creat personal room
                        await RoomUtilities.CreatePersonalRoomAsync(guild, user);

                        // Fluff
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

                        // Server announcement
                        await user.RemoveRoleAsync(newbie);
                        await lobby.SendMessageAsync($"Please welcome <@{user.Id}> to the server!");

                        // Tut room deletion
                        await message.Channel.SendMessageAsync("This channel will self-destruct in 2 minutes");
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;

                            Thread.Sleep(120000);
                             awaitableObject = (message.Channel as SocketTextChannel).DeleteAsync();

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
