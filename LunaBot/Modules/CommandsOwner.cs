using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LunaBot.Database;
using LunaBot.ServerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LunaBot.Modules
{
    class CommandsOwner : ModuleBase<SocketCommandContext>
    {
        [Command("ascend", RunMode = RunMode.Async)]
        public async Task AscendAsync(IUser requestedUser)
        {
            SocketUser author = Context.User;

            // User to ascend
            ulong parsedUserId = requestedUser.Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = author.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege != User.Privileges.Owner)
                {
                    Logger.Warning(author.Id.ToString(), "User tried to use ascend command and failed");
                    await ReplyAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }

                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                {
                    if (user.Privilege >= User.Privileges.Admin)
                    {
                        Logger.Info(author.Id.ToString(), $"User <@{requestedUser.Id}> already admin.");
                        await ReplyAsync($"<@{requestedUser.Id}> is already `admin` or above.");

                        return;
                    }
                    user.Privilege = User.Privileges.Admin;

                    SocketGuildChannel channel = Context.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Admin")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Moddlet")).FirstOrDefault()
                    };

                    await channel.Guild.GetUser(parsedUserId).AddRolesAsync(roles);

                    Logger.Info(author.Id.ToString(), $"Made <@{requestedUser.Id}> admin and moderator");
                    await ReplyAsync($"SHAPOW! <@{requestedUser.Id}> has been made `admin`!");
                }

                db.SaveChanges();

                await BotReporting.ReportAsync(ReportColors.ownerCommand,
                        (SocketTextChannel)Context.Channel,
                        $"Ascend Command by {Context.User.Username}",
                        $"<@{requestedUser.Id}> has been ascended to admin.",
                        Context.User,
                        (SocketUser)requestedUser).ConfigureAwait(false);
            }
        }

        [Command("descend", RunMode = RunMode.Async)]
        public async Task DescendAsync(IUser requestedUser)
        {
            SocketUser author = Context.User;

            // User to descend
            ulong parsedUserId = requestedUser.Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = author.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege != User.Privileges.Owner)
                {
                    Logger.Warning(author.Id.ToString(), "User tried to use descend command and failed");
                    await ReplyAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }

                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                {
                    if (user.Privilege == User.Privileges.User)
                    {
                        Logger.Info(author.Id.ToString(), $"User <@{requestedUser.Id}> not admin.");
                        await ReplyAsync($"<@{requestedUser.Id}> is not an `admin` or `moderator`.");
                    }
                    else if (user.Privilege == User.Privileges.Moderator)
                    {
                        Logger.Info(author.Id.ToString(), $"Removed moderator from <@{requestedUser.Id}>");
                        await ReplyAsync($"<@{requestedUser.Id}> is no longer `moderator`");
                    }
                    else
                    {
                        Logger.Info(author.Id.ToString(), $"Removed admin  and moderator from <@{requestedUser.Id}>");
                        await ReplyAsync($"<@{requestedUser.Id}> is no longer `admin` or `moderator`");
                    }

                    user.Privilege = User.Privileges.User;

                    SocketGuildChannel channel = Context.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Admin")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Moddlet")).FirstOrDefault()
                    };

                    await channel.Guild.GetUser(parsedUserId).RemoveRolesAsync(roles);
                }

                db.SaveChanges();

                await BotReporting.ReportAsync(ReportColors.ownerCommand,
                        (SocketTextChannel)Context.Channel,
                        $"Descend Command by {Context.User.Username}",
                        $"<@{requestedUser.Id}> has been descended to user.",
                        Context.User,
                        (SocketUser)requestedUser).ConfigureAwait(false);
            }

        }

        [Command("printColors", RunMode = RunMode.Async)]
        public async Task PrintColorsAsync()
        {
            if (UserIds.Owners.Contains<ulong>(Context.User.Id))
                return;

            EmbedBuilder eb = new EmbedBuilder();

            //foreach(PropertyInfo p in typeof(Color).GetProperties())
            //{
            //    if (p.PropertyType != typeof(Color))
            //    {
            //        continue;
            //    }

            //    eb.WithColor((Color) p.GetValue(null, null)); eb.WithTitle(p.Name); await ReplyAsync("", false, eb);
            //}

            eb.WithColor(Color.DarkerGrey); eb.WithTitle("DarkerGrey"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.DarkGrey); eb.WithTitle("DarkGrey"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.LighterGrey); eb.WithTitle("LighterGrey"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.LightGrey); eb.WithTitle("LightGrey"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.DarkRed); eb.WithTitle("DarkRed"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.Red); eb.WithTitle("Red"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.Orange); eb.WithTitle("Orange"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.LightOrange); eb.WithTitle("LightOrange"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.Gold); eb.WithTitle("Gold"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.DarkOrange); eb.WithTitle("DarkOrange"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.Magenta); eb.WithTitle("Magenta"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.DarkMagenta); eb.WithTitle("DarkMagenta"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.DarkTeal); eb.WithTitle("DarkTeal"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.Green); eb.WithTitle("Green"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.DarkGreen); eb.WithTitle("DarkGreen"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.Teal); eb.WithTitle("Teal"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.DarkBlue); eb.WithTitle("DarkBlue"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.Purple); eb.WithTitle("Purple"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.DarkPurple); eb.WithTitle("DarkPurple"); await ReplyAsync("", false, eb);
            eb.WithColor(Color.Blue); eb.WithTitle("Blue"); await ReplyAsync("", false, eb);


        }
    }
}
