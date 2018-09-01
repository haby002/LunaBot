using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Database;
using LunaBot.ServerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LunaBot.Modules
{
    class CommandsMod : ModuleBase<SocketCommandContext>
    {
        [Command("set", RunMode = RunMode.Async)]
        public async Task SetAsync(IUser requestedUser, string attribute, [Remainder] string content)
        {
            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;

                // check privileges
                ulong userId = author.Id;
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user.Privilege < User.Privileges.Moderator)
                {
                    Logger.Warning(author.Username, "Not enough permissions.");
                    await ReplyAsync("Can't let you do that Dave.");
                    return;
                }

                // Modify given user
                userId = requestedUser.Id;
                user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    switch (attribute.ToLower())
                    {
                        case "nick":
                        case "nickname":
                            SocketGuildUser gUser = (SocketGuildUser)requestedUser;
                            await gUser.ModifyAsync(u => u.Nickname = content);
                            user.Nickname = content;
                            Logger.Info(author.Username, $"Changed user <@{ requestedUser.Id}> 's nick from {user.Nickname} to {content}");
                            break;
                        case "description":
                        case "desc":
                            Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}>'s description from {user.Description} to {content}");
                            user.Description = content;
                            await ReplyAsync($"Success: <@{requestedUser.Id}>'s description updated to {content}");
                            break;
                        case "fur":
                        case "f":
                            Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}>'s fur from {user.Fur} to {content}");
                            user.Fur = content;
                            await ReplyAsync($"Success: <@{requestedUser.Id}>'s fur updated to {content}");
                            break;
                        case "level":
                        case "lvl":
                            if (int.TryParse(content, out int n))
                            {
                                Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}>'s level from {user.Level} to {content}");
                                user.Level = Convert.ToInt32(content);
                                user.Xp = 0;
                                await ReplyAsync($"Success: <@{requestedUser.Id}>'s level set to `{user.Level}`");
                            }
                            else
                            {
                                Logger.Warning(author.Username, "Failed database set level command");
                                await ReplyAsync($"Error: Level requires a number to set. You gave: `{content}`");
                            }
                            break;
                        case "xp":
                            if (int.TryParse(content, out int o))
                            {
                                Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}>'s xp from {user.Xp} to {content}");
                                user.Xp = Convert.ToInt32(content);
                                await ReplyAsync($"Success: <@{requestedUser.Id}>'s xp set to `{user.Xp}`");
                            }
                            else
                            {
                                Logger.Warning(author.Username, "Failed database set xp command");
                                await ReplyAsync($"Error: XP requires a number to set. You gave: `{content}`");
                            }
                            break;
                        case "age":
                        case "a":
                            if (int.TryParse(content, out int m))
                            {
                                Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}>'s age from {user.Age} to {content}");
                                user.Age = Convert.ToInt32(content);
                                await ReplyAsync($"Success: <@{requestedUser.Id}>'s age set to `{user.Age}`");
                            }
                            else
                            {
                                Logger.Warning(author.Username, "Failed database set age command");
                                await ReplyAsync($"Error: Age requires a number to set. You gave: `{content}`");
                            }
                            break;
                        case "gender":
                        case "g":
                            Predicate<SocketRole> genderFinder;
                            SocketRole gender;
                            SocketTextChannel channel = Context.Channel as SocketTextChannel;
                            List<SocketRole> roles = channel.Guild.Roles.ToList();
                            SocketGuildUser discordUser = requestedUser as SocketGuildUser;

                            User.Genders genderEnum = EnumParsers.StringToGender(content);
                            if (genderEnum == User.Genders.Null)
                            {
                                await ReplyAsync("I'm sorry I couldn't understand your message. Make sure the gender is either male, female, trans-male, trans-female, or other.\n" +
                                        $"You gave: {content}");

                                return;
                            }

                            // Remove old role
                            genderFinder = (SocketRole sr) => { return sr.Name == user.Gender.ToString().ToLower(); };
                            gender = roles.Find(genderFinder);
                            if (gender == null)
                            {
                                Logger.Error("System", $"Could not find user gender {user.Gender.ToString().ToString()}");
                            }
                            else
                            {
                                await discordUser.RemoveRoleAsync(gender);
                            }

                            // Add new role
                            genderFinder = (SocketRole sr) => { return sr.Name == genderEnum.ToString().ToLower(); };
                            gender = roles.Find(genderFinder);
                            await discordUser.AddRoleAsync(gender);

                            user.Gender = genderEnum;
                            db.SaveChanges();

                            Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}>'s gender from {user.Gender} to {content}");
                            await ReplyAsync($"Success: <@{requestedUser.Id}>'s gender set to `{user.Gender}`");
                            break;
                        case "orientation":
                        case "o":
                            Predicate<SocketRole> orientationFinder;
                            SocketRole orientation;
                            channel = Context.Channel as SocketTextChannel;
                            roles = channel.Guild.Roles.ToList();
                            discordUser = requestedUser as SocketGuildUser;

                            User.Orientation orientationEnum = EnumParsers.StringToOrientation(content);
                            if (orientationEnum == User.Orientation.None)
                            {
                                await ReplyAsync("Couldn't understand that gender... it can either be\n" +
                                    "```\n" +
                                    "- Male\n" +
                                    "- Female\n" +
                                    "- Trans-Female\n" +
                                    "- Transe-Male\n" +
                                    "- Other\n" +
                                    "```");

                                return;
                            }

                            // Remove old role
                            orientationFinder = (SocketRole sr) => { return sr.Name == user.orientation.ToString().ToLower(); };
                            orientation = roles.Find(orientationFinder);
                            if (orientation == null)
                            {
                                Logger.Error("System", $"Could not find user orientation {user.orientation.ToString().ToString()}");
                            }
                            else
                            {
                                await discordUser.RemoveRoleAsync(orientation);
                            }

                            // Add new role
                            orientationFinder = (SocketRole sr) => { return sr.Name == orientationEnum.ToString().ToLower(); };
                            orientation = roles.Find(orientationFinder);
                            await discordUser.AddRoleAsync(orientation);

                            user.orientation = orientationEnum;
                            db.SaveChanges();

                            Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}>'s orientation to {user.orientation.ToString()}");
                            await ReplyAsync($"Success: <@{requestedUser.Id}>'s orientation set to `{user.orientation.ToString()}`");
                            break;

                        case "ref":
                        case "r":
                            if (Uri.TryCreate(content, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                            {
                                Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}>'s ref from {user.Ref} to {content}");
                                user.Ref = content;
                                await ReplyAsync($"Success: <@{requestedUser.Id}>'s ref has been updated");
                            }
                            else
                            {
                                Logger.Warning(author.Username, "Failed database set ref command");
                                await ReplyAsync($"Error: Ref sheet must be a link. You gave: `{content}`");
                            }
                            break;
                        case "forcetut":
                            bool value;
                            if(!Boolean.TryParse(content, out value))
                            {
                                await ReplyAsync($"Could not set `forcetut` to {value}. It can be either `true` or `false`");
                                Logger.Info(author.Username, $"Failed to change user <@{requestedUser.Id}> 's tutorial state from {user.TutorialFinished} to {value}");
                                return;
                            }
                            Logger.Info(author.Username, $"Changed user <@{requestedUser.Id}> 's tutorial state from {user.TutorialFinished} to {value}");
                            await ReplyAsync($"Changed <@{requestedUser.Id}>'s tutorial state from {user.TutorialFinished} to {value}");
                            user.TutorialFinished = value;
                            break;
                        default:
                            Logger.Warning(author.Username, "Failed database set command.");
                            await ReplyAsync($"Error: Could not find attribute {attribute}. Check you syntax!");
                            return;
                    }

                    db.SaveChanges();
                    Logger.Verbose(author.Username, $"Updated data for {userId}");

                    await BotReporting.ReportAsync(ReportColors.modCommand,
                        (SocketTextChannel)Context.Channel,
                        $"Mod Attribute Set by {Context.User.Username}",
                        $"<@{Context.User.Id}> has changed <@{requestedUser.Id}>'s {attribute} to `{content}` ",
                        Context.User,
                        (SocketUser)requestedUser).ConfigureAwait(false);

                    return;
                }

                Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: {userId}");

                //Logger.Verbose(author.Username, "Created User");
                //ReplyAsync("Created User");

                Logger.Verbose("System", $"Updated information for user {author}");
            }
        }

        [Command("registerall", RunMode = RunMode.Async)]
        public async Task RegisterAllAsync()
        {
            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;
                ulong userId = author.Id;

                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege == User.Privileges.User)
                {
                    Logger.Warning(author.Username, "Failed RegisterAll command");
                    await ReplyAsync("You're not a moderator, go away.");

                    return;
                }

                Logger.Info(author.Username, "Fixing Registrations");
                await ReplyAsync("Fixing registrations...");

                SocketGuildChannel channel = Context.Channel as SocketGuildChannel;
                List<SocketGuildUser> users = channel.Guild.Users.ToList();

                foreach (SocketGuildUser u in users)
                {
                    if (db.Users.Where(x => x.DiscordId == u.Id).Count() == 0)
                    {
                        Logger.Verbose(author.Username, $"Creating User Data for {u.Username}");

                        User newUser = new User();
                        newUser.DiscordId = u.Id;
                        newUser.Level = 1;
                        newUser.Privilege = 0;
                        newUser.TutorialFinished = false;
                        newUser.Gender = User.Genders.Null;
                        db.Users.Add(newUser);
                        var list = db.Users.ToList();

                        Logger.Verbose(author.Username, $"Created User {newUser.ID.ToString()}");

                    }
                }

                db.SaveChanges();

                await ReplyAsync("Finished registering users.");
                Logger.Info(author.Username, "Finished registering users.");

                await BotReporting.ReportAsync(ReportColors.modCommand,
                        (SocketTextChannel)Context.Channel,
                        $"RegisterAll command by {Context.User.Username}",
                        $"",
                        Context.User).ConfigureAwait(false);

            }
        }

        [Command("forcetut", RunMode = RunMode.Async)]
        public async Task ForcetutAsync(IUser requestedUser)
        {
            // User to forcetut
            ulong parsedUserId = requestedUser.Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                SocketGuildUser discordUser = requestedUser as SocketGuildUser;

                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege == User.Privileges.User)
                {
                    Logger.Warning(Context.User.Id.ToString(), "User tried to use forcetut command and failed");
                    await ReplyAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }
                
                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                
                // Remove old gender and orientation
                Predicate<SocketRole> genderFinder = (SocketRole sr) => { return sr.Name == user.Gender.ToString().ToLower(); };
                SocketRole genderRole = Context.Guild.Roles.ToList().Find(genderFinder);
                if (genderRole != null)
                {
                    await ((SocketGuildUser)requestedUser).RemoveRoleAsync(genderRole);
                    Logger.Verbose("System", $"found role {genderRole.Name} and removed it.");
                }
                else
                {
                    Logger.Warning("System", $"Couldn't find role {user.Gender.ToString().ToLower()}.");
                }

                Predicate<SocketRole> orientationFinder = (SocketRole sr) => { return sr.Name == user.orientation.ToString().ToLower(); };
                SocketRole orientationRole = Context.Guild.Roles.ToList().Find(orientationFinder);
                if (orientationRole != null)
                {
                    await ((SocketGuildUser)requestedUser).RemoveRoleAsync(orientationRole);
                    Logger.Verbose("System", $"found role {orientationRole.Name} and removed it.");
                }
                else
                {
                    Logger.Warning("System", $"Couldn't find role {user.orientation.ToString().ToLower()}.");
                }

                //Reset database entry for user
                user.ResetUser();
                UserUtilities.manualRegister(discordUser);

                SocketGuildChannel channel = Context.Channel as SocketGuildChannel;
                IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                SocketRole role = guildRoles.Where(x => x.Name.Equals("Newbie")).FirstOrDefault();
                await channel.Guild.GetUser((ulong)parsedUserId).AddRoleAsync(role);

                // Creat intro room
                RestTextChannel introRoom = await channel.Guild.CreateTextChannelAsync($"intro-{parsedUserId}");

                await Task.Run(async () =>
                {
                    // Make room only visible to new user, staff, and Luna
                    await introRoom.AddPermissionOverwriteAsync(discordUser, Permissions.userPerm);

                    // Start interaction with user. Sleeps are for humanizing the bot.
                    await introRoom.SendMessageAsync("Welcome to the server! Lets get you settled, alright?");
                    Thread.Sleep(1000);
                    await introRoom.SendMessageAsync("Firstly, what should we call you?\n" +
                "If you'd rather not change your nick just type `none`");
                }).ConfigureAwait(false);

                db.SaveChanges();

                await BotReporting.ReportAsync(ReportColors.modCommand,
                        (SocketTextChannel)Context.Channel,
                        $"Forcetut by {Context.User.Username}",
                        $"<@{Context.User.Id}> has placed <@{requestedUser.Id}> in a tutorial room.",
                        Context.User,
                        (SocketUser)requestedUser).ConfigureAwait(false);
            }
        }

        [Command("timeout", RunMode = RunMode.Async)]
        public async Task TimeoutAsync(IUser requestedUser, int minutes)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege == 0)
                {
                    Logger.Warning(Context.User.Username, "Failed timout command. Not enough privileges.");
                    await ReplyAsync("You're not a moderator, go away.");

                    return;
                }

                await MuteUserHelper.MuteAsync(Context.Channel as SocketTextChannel, requestedUser as SocketGuildUser, minutes);
                
                await BotReporting.ReportAsync(ReportColors.modCommand,
                    (SocketTextChannel)Context.Channel,
                    $"Timeout command by {Context.User.Username}",
                    $"<@{Context.User.Id}> has muted <@{requestedUser.Id}> for `{minutes}` minutes.",
                    Context.User,
                    (SocketUser)requestedUser).ConfigureAwait(false);
            }
        }

        [Command("intervention", RunMode = RunMode.Async)]
        public async Task InterventionAsync(IUser requestedUser)
        {
            await BotReporting.ReportAsync(ReportColors.modCommand,
                        (SocketTextChannel)Context.Channel,
                        $"Intervention command by {Context.User.Username}",
                        $"<@{Context.User.Id}> placed <@{requestedUser.Id}> in an intervention.",
                        Context.User,
                        (SocketUser)requestedUser).ConfigureAwait(false);

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege == User.Privileges.User)
                {
                    Logger.Warning(Context.User.Username, "Failed intervention command. Not enough privileges.");
                    await ReplyAsync("You are not a moderator, go away.");

                    return;
                }

                try
                {
                    // Look for intervention role
                    SocketRole intervetionRole = Context.Guild.Roles.Where(x => x.Name.ToLower() == "intervention").First();

                    // give user the role
                    SocketGuildUser user = requestedUser as SocketGuildUser;
                    await user.AddRoleAsync(intervetionRole);

                    // Create the channel and add the user
                    RestTextChannel interventionChannel = await Context.Guild.CreateTextChannelAsync($"intervention-{user.Id}");
                    await interventionChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Permissions.removeAllPerm);
                    await interventionChannel.AddPermissionOverwriteAsync(user, Permissions.userPerm);
                    await interventionChannel.AddPermissionOverwriteAsync(Context.Guild.Roles.Where(r => r.Name == Roles.Staff).FirstOrDefault(), Permissions.adminPerm);

                    // Notify user of intervention
                    await interventionChannel.SendMessageAsync($"{user.Mention} you have been placed in intervention due to either your profile or attitude in the server.\n" +
                        $"A staff member should be here shortly to assist you.");
                }
                catch (Exception e)
                {
                    await BotReporting.ReportAsync(ReportColors.exception,
                    (SocketTextChannel)Context.Channel,
                    e.Message,
                    $"Source: {e.Source}\n {e.InnerException}\n {e.StackTrace}",
                    Engine.luna);

                    return;
                }

                
                // Add permissions (user and staff)
                // un-limbo person

            }
        }

        [Command("ban", RunMode = RunMode.Async)]
        public async Task BanAsync(IUser requestedUser, [Remainder] string reason = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < User.Privileges.Moderator)
                {
                    Logger.Warning(Context.User.Username, "User tried to use ban command and failed");
                    await ReplyAsync($"Looks like someone wants to *get* a ban. Call a mod will ya?");
                    return;
                }

                if (reason == null)
                {
                    await Context.Guild.AddBanAsync(requestedUser, 0, $"Banned by {Context.User.Username}: no reason given");

                    return;
                }

                await Context.Guild.AddBanAsync(requestedUser, 0, $"Banned by {Context.User.Username}: {reason}");

                await BotReporting.ReportAsync(ReportColors.adminCommand,
                        (SocketTextChannel)Context.Channel,
                        $"Ban Command by {Context.User.Username}",
                        $"<@{requestedUser.Id}> has been kicked. Reason: `{reason}`",
                        Context.User,
                        (SocketUser)requestedUser).ConfigureAwait(false);
            }
        }

        [Command("kick", RunMode = RunMode.Async)]
        public async Task KickAsync(IUser requestedUser, [Remainder] string reason = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < User.Privileges.Moderator)
                {
                    Logger.Warning(Context.User.Username, "User tried to use kick command and failed");
                    await ReplyAsync($"No can do Jonny boy. You need moddlet for that.");
                    return;
                }

                await KickUserHelper.KickAsync(Context.Channel as SocketTextChannel, requestedUser as SocketGuildUser);
                Logger.Warning(Context.User.Username, $"Kicked {requestedUser.Username} by {Context.User.Username}");

                await BotReporting.ReportAsync(ReportColors.adminCommand,
                        (SocketTextChannel)Context.Channel,
                        $"Kick Command by {Context.User.Username}",
                        $"<@{requestedUser.Id}> has been kicked",
                        Context.User,
                        (SocketUser)requestedUser).ConfigureAwait(false);
            }
        }

        [Command("warn", RunMode = RunMode.Async)]
        public async Task warnAsync(IUser requestedUser)
        {
            using(DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                if(db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < User.Privileges.Moderator)
                {
                    Logger.Warning(Context.User.Username, "User tried to use warn command and failed");
                    await ReplyAsync($"You wanna get warned? Cause that's how you get warned.");
                    return;
                }

                User databaseUser = db.Users.Where(x => x.DiscordId == requestedUser.Id).FirstOrDefault();

                databaseUser.warnCount++;

                // When a user reaches 5 warns they will be kicked.
                if (databaseUser.warnCount % 5 == 0)
                {
                    await ReplyAsync($"<@{requestedUser.Id}>, we warned you and you didn't listen. Goodbye.");
                    await KickUserHelper.KickAsync((SocketTextChannel)Context.Channel, (SocketGuildUser)requestedUser);
                }
                else
                {
                    await ReplyAsync($"<@{requestedUser.Id}> you have been warned. Current: {databaseUser.warnCount}, get 5 and you *will* be kicked.");
                }

                db.SaveChanges();

                await BotReporting.ReportAsync(ReportColors.modCommand,
                                            (SocketTextChannel)Context.Channel,
                                            $"{Context.User.Username} used warn command",
                                            $"{Context.User.Username} warned {requestedUser.Username} in {Context.Channel.Name}.",
                                            Context.User,
                                            (SocketGuildUser)requestedUser,
                                            $"Mod ID: {Context.User.Id}");
            }
        }

        [Command("removeWarn", RunMode = RunMode.Async)]
        public async Task removeWarnAsync(IUser requestedUser, int amount = 0)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < User.Privileges.Moderator)
                {
                    Logger.Warning(Context.User.Username, "User tried to use removeWarn command and failed");
                    await ReplyAsync($"Tough luck kiddo, you aint got any powers over me.");
                    return;
                }
                if(amount < 0)
                {
                    await ReplyAsync($"I can't remove a negative number of warns. You gave me `{amount}`");
                    return;
                }

                User databaseUser = db.Users.Where(x => x.DiscordId == requestedUser.Id).FirstOrDefault();

                databaseUser.warnCount = amount == 0 ? databaseUser.warnCount - 1 : databaseUser.warnCount - amount;

                if (databaseUser.warnCount < 0)
                {
                    databaseUser.warnCount = 0;
                }

                await ReplyAsync($"<@{requestedUser.Id}> warnings reduced to: {databaseUser.warnCount}");


                await BotReporting.ReportAsync(ReportColors.modCommand,
                                            (SocketTextChannel)Context.Channel,
                                            $"{Context.User.Username} used removeWarn command",
                                            $"{Context.User.Username} set warns for {requestedUser.Username} to {databaseUser.warnCount} in {Context.Channel.Name}.",
                                            Context.User,
                                            (SocketGuildUser)requestedUser,
                                            $"Mod ID: {Context.User.Id}");

                db.SaveChanges();
            }
        }

        [Command("serverstats", RunMode = RunMode.Async)]
        public async Task serverStatsAsync()
        {
            EmbedBuilder eb = new EmbedBuilder();

            using (DiscordContext db = new DiscordContext())
            {
                ulong authorId = Context.User.Id;

                User author = db.Users.Where(u => (ulong)u.DiscordId == authorId).FirstOrDefault();

                if(author == null)
                {
                    Logger.Warning("system", $"Cannot find user {Context.User.Username}");
                    return;
                }

                if (author.Privilege < User.Privileges.Moderator)
                {
                    await ReplyAsync("Go bug someone else.");

                    return;
                }


                EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder();
                authorBuilder.WithName(Context.Guild.Name);
                authorBuilder.WithIconUrl(Context.Guild.IconUrl);

                eb.WithAuthor(authorBuilder);
                eb.WithColor(Color.Teal);
                db.Users.Count(u => u.Gender == User.Genders.Male);
                eb.WithDescription(
                    $"Users: {Context.Guild.Users.Count}\n" +
                    $"Rooms: {Context.Guild.TextChannels.Count}\n" +
                    $"Voice Channels: {Context.Guild.VoiceChannels.Count}\n" +
                    $"5+ Levels: {db.Users.Count(u => u.Level >= 5)}" +
                    $"18+ Users: {db.Users.Count(u => u.Age >= 18)}\n" +
                    $"Monk Role: {db.Users.Count(u => u.Monk.Equals(true))}\n" +
                    $"NSFW Role: {db.Users.Count(u => u.Nsfw.Equals(true))}" +
                    $"Subscribed to Games: {db.Users.Count(u => u.Games.Equals(true))}\n" +
                    $"Subscribed to Bot Updates: {db.Users.Count(u => u.BotUpdates.Equals(true))}\n" +
                    $"Genders\n" +
                    $"Male:{db.Users.Count(u => u.Gender == User.Genders.Male) + db.Users.Count(u => u.Gender == User.Genders.Trans_Male)}, " +
                    $"Female: {db.Users.Count(u => u.Gender == User.Genders.Female) + db.Users.Count(u => u.Gender == User.Genders.Trans_Female)}" +
                    $"Other: {db.Users.Count(u => u.Gender == User.Genders.Other)}\n" +
                    $"Fluid: {db.Users.Count(u => u.Gender == User.Genders.Fluid)}" + $"None: {db.Users.Count(u => u.Gender == User.Genders.None)}");
                eb.WithCurrentTimestamp();

                EmbedFooterBuilder footer = new EmbedFooterBuilder();
                footer.WithText($"Server ID: {Context.Guild.Id}");
                eb.WithFooter(footer);

                eb.WithThumbnailUrl(Context.Guild.IconUrl);
            }

            await ReplyAsync("", false /*TTS*/, eb);
        }

        [Command("clear", RunMode = RunMode.Async)]
        public async Task clearAsync(int amount = 1)
        {
            using (DiscordContext db = new DiscordContext())
            {
                User author = db.Users.Where(u => u.DiscordId == Context.User.Id).FirstOrDefault();

                if(author == null)
                {
                    await BotReporting.ReportAsync(ReportColors.exception, 
                        Context.Channel as SocketTextChannel, 
                        "Cannot find user", 
                        $"Cannot find `{Context.User.Username}` in the database.", 
                        Engine.luna, 
                        Context.User);

                    await ReplyAsync($"Cannot find {Context.User.Username} in the database.");

                    return;
                }
                else if(author.Privilege < User.Privileges.Moderator)
                {
                    await BotReporting.ReportAsync(ReportColors.modCommand,
                        Context.Channel as SocketTextChannel,
                        "User attempted mod command",
                        $"{Context.User.Username} attempted to use `clear` command in `{Context.Channel.Name}`\n" +
                        $"Message: `{Context.Message.Content}`",
                        Engine.luna,
                        Context.User);

                    await ReplyAsync($"Want me to clear *you* from this server?");

                    return;
                }

                await BotReporting.ReportAsync(ReportColors.modCommand,
                        Context.Channel as SocketTextChannel,
                        $"Clear all command",
                        $"{Context.User.Username} used `clear` command in `{Context.Channel.Name}` to clear `{amount}` messages.",
                        Engine.luna,
                        Context.User);

                var allMessages = await Context.Channel.GetMessagesAsync(amount + 1).Flatten();
                
                IUserMessage replyMessage = await ReplyAsync($"Deleting last `{amount}` messages");

                await Context.Channel.DeleteMessagesAsync(allMessages);

                Task.Run(async () =>
                {
                    Thread.Sleep(2000);
                    await replyMessage.DeleteAsync();

                }).Start();
            }
        }

        [Command("fixroles", RunMode = RunMode.Async)]
        public async Task fixRolesAsync()
        {
            using (DiscordContext db = new DiscordContext())
            {
                User author = db.Users.Where(u => u.DiscordId == Context.User.Id).FirstOrDefault();

                if (author == null)
                {
                    await BotReporting.ReportAsync(ReportColors.exception,
                        Context.Channel as SocketTextChannel,
                        "Cannot find user",
                        $"Cannot find `{Context.User.Username}` in the database.",
                        Engine.luna,
                        Context.User);

                    await ReplyAsync($"Cannot find {Context.User.Username} in the database.");

                    return;
                }
                else if (author.Privilege < User.Privileges.Moderator)
                {
                    await BotReporting.ReportAsync(ReportColors.modCommand,
                        Context.Channel as SocketTextChannel,
                        "User attempted mod command",
                        $"{Context.User.Username} attempted to use `fixroles` command in `{Context.Channel.Name}`:\n" +
                        $"Message: `{Context.Message.Content}`",
                        Engine.luna,
                        Context.User);

                    await ReplyAsync($"Get out of here chump");

                    return;
                }

                await BotReporting.ReportAsync(ReportColors.modCommand,
                        Context.Channel as SocketTextChannel,
                        $"Clear all command",
                        $"{Context.User.Username} used `fixroles` command in `{Context.Channel.Name}`.",
                        Engine.luna,
                        Context.User);

                await ReplyAsync("Applying `Verified` role to users above level 10...");

                SocketRole verifiedRole = Context.Guild.Roles.Where(r => r.Name == Roles.Verified).FirstOrDefault();

                if(verifiedRole == null)
                {
                    await ReplyAsync("Verified role not found!");

                    return;
                }

                foreach(User user in db.Users.Where(u => u.Level >= 10))
                {
                    await Context.Guild.GetUser(user.DiscordId).AddRoleAsync(verifiedRole);
                }

                await ReplyAsync("Finished adding `Verified` role.");
            }
        }
    }
}
