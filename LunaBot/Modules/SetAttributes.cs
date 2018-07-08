using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LunaBot.Database;
using LunaBot.ServerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Modules
{
    class SetAttributes : ModuleBase<SocketCommandContext>
    {
        [Command("desc", RunMode = RunMode.Async)]
        public async Task SetDescAsync([Remainder]string parameter)
        {
            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;
                ulong userId = author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);

                if (user != null)
                {
                    Logger.Verbose(author.Username, $"Setting description for {userId} to: {parameter}");

                    user.Description = parameter;
                    db.SaveChanges();

                    await ReplyAsync($"Changed <@{userId}>'s description to: {user.Description}");

                    return;
                }

                Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{author.Username}`");

            }
        }

        [Command("d", RunMode = RunMode.Async)]
        public async Task SetDescriptionExtensionAsync([Remainder]string parameter)
        {
            await SetDescAsync(parameter);
        }

        [Command("description", RunMode = RunMode.Async)]
        public async Task SetDescriptionLongExtensionAsync([Remainder]string parameter)
        {
            await SetDescAsync(parameter);
        }

        [Command("age", RunMode = RunMode.Async)]
        public async Task SetAgeAsync(int age)
        {
            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;
                ulong userId = author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Verbose(author.Username, $"Setting {userId} description to {age}");

                    user.Age = age;
                    db.SaveChanges();

                    await ReplyAsync($"Changed <@{userId}> age to `{user.Age}` years old.");

                    return;
                }

                Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{author.Username}`");

            }
        }

        [Command("a", RunMode = RunMode.Async)]
        public async Task SetAgeExtensionAsync(int age)
        {
            await SetAgeAsync(age);
        }

        [Command("fur", RunMode = RunMode.Async)]
        public async Task SetFurAsync([Remainder] string parameter)
        {
            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;
                ulong userId = author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(author.Username, $"user <@{userId}>'s fur set to: {parameter}");

                    user.Fur = parameter;
                    db.SaveChanges();

                    await ReplyAsync($"<@{userId}> fur set to {parameter}");

                    return;
                }

                Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{author.Username}`");

            }
        }

        [Command("f", RunMode = RunMode.Async)]
        public async Task SetFurExtensionAsync([Remainder] string parameter)
        {
            await SetFurAsync(parameter);
        }

        [Command("g", RunMode = RunMode.Async)]
        public async Task SetGenderAsync(string parameter)
        {
            User.Genders gender = EnumParsers.StringToGender(parameter);
            if (gender == User.Genders.Null)
            {
                await ReplyAsync("Couldn't understand that gender... it can either be\n" +
                    "```\n" +
                    "- Male\n" +
                    "- Female\n" +
                    "- Trans-Female\n" +
                    "- Trans-Male\n" +
                    "- Other\n" +
                    "- None \n" +
                    "- Fluid \n" +
                    "```");

                return;
            }

            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;
                ulong userId = author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning("AdminSetCmd", $"Setting @<{userId}>'s gender to {gender.ToString()}.");

                    SocketGuildChannel guildChannel = Context.Channel as SocketGuildChannel;
                    List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

                    // Remove old role
                    Predicate<SocketRole> genderFinder = (SocketRole sr) => { return sr.Name == user.Gender.ToString().ToLower(); };
                    SocketRole genderRole = roles.Find(genderFinder);
                    genderRole = roles.Find(genderFinder);
                    if (genderRole != null)
                    {
                        await guildChannel.GetUser((ulong)userId).RemoveRoleAsync(genderRole);
                        Logger.Verbose("System", $"found role {genderRole.Name} and removed it.");
                    }
                    else
                    {
                        Logger.Warning("System", $"Couldn't find role {user.Gender.ToString().ToLower()}.");
                    }

                    // Add new role
                    genderFinder = (SocketRole sr) => { return sr.Name == gender.ToString().ToLower(); };
                    genderRole = roles.Find(genderFinder);
                    await guildChannel.GetUser((ulong)userId).AddRoleAsync(genderRole);

                    user.Gender = gender;
                    db.SaveChanges();

                    await ReplyAsync($"Changed <@{userId}>'s gender to {gender.ToString().ToLower()}");

                    return;
                }

                Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{author.Username}`");

            }
        }
        
        [Command("gender", RunMode = RunMode.Async)]
        public async Task SetGenderExtensionAsync(string parameter)
        {
            await SetGenderAsync(parameter);
        }

        [Command("o", RunMode = RunMode.Async)]
        public async Task SetOrientationAsync(string parameter)
        {
            User.Orientation orientation = EnumParsers.StringToOrientation(parameter);

            if (orientation == User.Orientation.None)
            {
                await ReplyAsync("Couldn't understand that orientation... it can either be\n" +
                    "```\n" +
                    "- Straight\n" +
                    "- Gay\n" +
                    "- Bisexual\n" +
                    "- Asexual\n" +
                    "- Pansexual\n" +
                    "- Gray-a (if you'd rather it not be shown)\n" +
                    "```");

                return;
            }

            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;
                ulong userId = author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Info(author.Username, $"Changing @<{userId}> orientation to {orientation.ToString()}.");

                    SocketGuildChannel guildChannel = Context.Channel as SocketGuildChannel;
                    List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

                    // Remove old role
                    Predicate<SocketRole> orientationFinder = (SocketRole sr) => { return sr.Name == user.orientation.ToString().ToLower(); };
                    SocketRole orientationRole = roles.Find(orientationFinder);
                    if (orientationRole != null)
                    {
                        await guildChannel.GetUser(userId).RemoveRoleAsync(orientationRole);
                        Logger.Verbose("System", $"found role {orientationRole.Name} and removed it.");
                    }
                    else
                    {
                        Logger.Warning("System", $"Couldn't find role {user.orientation.ToString().ToLower()}.");
                    }

                    // Adding role to user
                    orientationFinder = (SocketRole sr) => { return sr.Name == orientation.ToString().ToLower(); };
                    orientationRole = roles.Find(orientationFinder);
                    await guildChannel.GetUser(userId).AddRoleAsync(orientationRole);


                    user.orientation = orientation;
                    db.SaveChanges();

                    await ReplyAsync($"<@{userId}> orientation is now {user.orientation.ToString().ToLower()}");

                    return;
                }

                Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{author.Username}`");

            }
        }

        [Command("orientation", RunMode = RunMode.Async)]
        public async Task SetOrientationExtensionAsync(string parameter)
        {
            await SetOrientationAsync(parameter);
        }

        [Command("ref", RunMode = RunMode.Async)]
        public async Task SetRefAsync([Remainder] string parameter)
        {
            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;
                ulong userId = author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(author.Username, $"Setting <@{userId}>'s ref to {parameter}");

                    user.Ref = parameter;
                    db.SaveChanges();

                    await ReplyAsync($"<@{userId}>'s ref has been set to {parameter}");

                    return;
                }

                Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{author.Username}`");

            }
        }

        [Command("sfw", RunMode = RunMode.Async)]
        public async Task SetSfwAsync(string parameter)
        {
            SocketUser author = Context.User;
            ulong userId = author.Id;

            SocketGuildChannel guildChannel = Context.Channel as SocketGuildChannel;
            List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

            if (parameter == "no")
            {
                Logger.Info(author.Username, $"Removing SFW role for <@{userId}>.");

                // Remove old role
                Predicate<SocketRole> SfwFinder = (SocketRole sr) => { return sr.Name == "SFW"; };
                SocketRole SfwRole = roles.Find(SfwFinder);
                if (SfwRole != null)
                {
                    await guildChannel.GetUser(userId).RemoveRoleAsync(SfwRole);
                    Logger.Verbose("System", $"found role {SfwRole.Name} and removed it.");
                    await ReplyAsync($"<@{userId}> is now alowed into the NSFW rooms.");
                }
                else
                {
                    Logger.Warning("System", $"Couldn't find role SWF.");
                }
            }
            else if (parameter == "yes")
            {
                // Adding role to user
                Predicate<SocketRole> Sfwfinder = (SocketRole sr) => { return sr.Name == "SFW"; };
                SocketRole SfwRole = roles.Find(Sfwfinder);
                await guildChannel.GetUser(userId).AddRoleAsync(SfwRole);
                if (SfwRole != null)
                {
                    await ReplyAsync($"<@{userId}> has been removed from the NSFW rooms.");
                }
                else
                {
                    Logger.Warning("System", $"Couldn't find role SWF.");
                }
            }
            else
            {
                await ReplyAsync($"Sorry I couldn't understand. Please answer `yes` or `no`");
                return;
            }
        }

        [Command("monk", RunMode = RunMode.Async)]
        public async Task SetMonkAsync(string parameter)
        {
            SocketUser author = Context.User;
            ulong userId = author.Id;

            SocketGuildChannel guildChannel = Context.Channel as SocketGuildChannel;
            List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

            if (parameter == "no")
            {
                Logger.Info(author.Username, $"Removing Monk role for <@{userId}>.");

                // Remove old role
                Predicate<SocketRole> MonkFinder = (SocketRole sr) => { return sr.Name == "Monk"; };
                SocketRole MonkRole = roles.Find(MonkFinder);
                if (MonkRole != null)
                {
                    await guildChannel.GetUser(userId).RemoveRoleAsync(MonkRole);
                    Logger.Verbose("System", $"found role {MonkRole.Name} and removed it.");
                    await ReplyAsync($"<@{userId}> is now alowed into the RP rooms.");
                }
                else
                {
                    Logger.Warning("System", $"Couldn't find role Monk.");
                }
            }
            else if (parameter == "yes")
            {


                // Adding role to user
                Predicate<SocketRole> Monkfinder = (SocketRole sr) => { return sr.Name == "Monk"; };
                SocketRole MonkRole = roles.Find(Monkfinder);
                await guildChannel.GetUser(userId).AddRoleAsync(MonkRole);
                if (MonkRole != null)
                {
                    await ReplyAsync($"<@{userId}> has been removed from RP rooms.");
                }
                else
                {
                    Logger.Warning("System", $"Couldn't find role Monk.");
                }
            }
            else
            {
                await ReplyAsync($"Sorry I couldn't understand. Please answer `yes` or `no`");

            }
        }
        
        [Command("games", RunMode = RunMode.Async)]
        public async Task SetGamesAsync()
        {
            SocketUser author = Context.User;
            ulong userId = author.Id;

            SocketGuildChannel guildChannel = Context.Channel as SocketGuildChannel;
            List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

            using (DiscordContext db = new DiscordContext())
            {
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);

                if (user != null)
                {
                    Predicate<SocketRole> RoleFinder = (SocketRole sr) => { return sr.Name == Roles.Games; };
                    SocketRole AnnouncementRole = roles.Find(RoleFinder);
                    AnnouncementRole = roles.Find(RoleFinder);

                    // Check if role was found on the server
                    if(AnnouncementRole == null)
                    {
                        Logger.Warning("System", $"Couldn't find role {Roles.Games}");
                        await BotReporting.ReportAsync(ReportColors.exception, Context.Channel as SocketTextChannel, "Error finding Role.", $"Could not find role: {Roles.Games}, contact admin.", Context.User);
                        await ReplyAsync("Error finding role, notifying staff.");
                    }
                    
                    if (user.Games == true)
                    {
                        // Remove the role
                        await guildChannel.GetUser((ulong)userId).RemoveRoleAsync(AnnouncementRole);
                        Logger.Verbose("System", $"found role {AnnouncementRole.Name} and removed it.");
                        user.Games = false;

                        await ReplyAsync($"<@{userId}> left the `{Roles.Games}` role.");
                    }
                    else
                    {
                        // Add the role
                        await guildChannel.GetUser((ulong)userId).AddRoleAsync(AnnouncementRole);
                        Logger.Verbose("System", $"Found role {AnnouncementRole.Name} and added it.");
                        user.Games = true;

                        await ReplyAsync($"<@{userId}> joined the `{Roles.Games}` role.");
                    }

                    db.SaveChanges();

                }
                else
                {
                    Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                    await ReplyAsync($"Failed to find user: `{author.Username}`");
                }
            }
        }

        [Command("botupdates", RunMode = RunMode.Async)]
        public async Task SetBotsUpdatesAsync()
        {
            SocketUser author = Context.User;
            ulong userId = author.Id;

            SocketGuildChannel guildChannel = Context.Channel as SocketGuildChannel;
            List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

            using (DiscordContext db = new DiscordContext())
            {
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);

                if (user != null)
                {
                    Predicate<SocketRole> RoleFinder = (SocketRole sr) => { return sr.Name == Roles.BotUpdates; };
                    SocketRole AnnouncementRole = roles.Find(RoleFinder);
                    AnnouncementRole = roles.Find(RoleFinder);

                    // Check if role was found on the server
                    if (AnnouncementRole == null)
                    {
                        Logger.Warning("System", $"Couldn't find role {Roles.BotUpdates}");
                        await BotReporting.ReportAsync(ReportColors.exception, Context.Channel as SocketTextChannel, "Error finding Role.", $"Could not find role: {Roles.BotUpdates}, contact admin.", Context.User);
                        await ReplyAsync("Error finding role, notifying staff.");
                    }

                    if (user.BotUpdates == true)
                    {
                        // Remove the role
                        await guildChannel.GetUser((ulong)userId).RemoveRoleAsync(AnnouncementRole);
                        Logger.Verbose("System", $"found role {AnnouncementRole.Name} and removed it.");
                        user.BotUpdates = false;

                        await ReplyAsync($"<@{userId}> left the `{Roles.BotUpdates}` role.");
                    }
                    else
                    {
                        // Add the role
                        await guildChannel.GetUser((ulong)userId).AddRoleAsync(AnnouncementRole);
                        Logger.Verbose("System", $"Found role {AnnouncementRole.Name} and added it.");
                        user.BotUpdates = true;

                        await ReplyAsync($"<@{userId}> joined the `{Roles.BotUpdates}` role.");
                    }

                    db.SaveChanges();

                }
                else
                {
                    Logger.Verbose(author.Username, $"Failed to find user: {userId}");
                    await ReplyAsync($"Failed to find user: `{author.Username}`");
                }
            }
        }
    }
}
