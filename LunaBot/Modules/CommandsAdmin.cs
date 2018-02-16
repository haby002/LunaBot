using Discord;
using Discord.Commands;
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
    class CommandsAdmin : ModuleBase<SocketCommandContext>
    {
        [Command("demote", RunMode = RunMode.Async)]
        public async Task DemoteAsync(IUser requestedUser)
        {
            SocketUser author = Context.User;

            // User to demote
            ulong parsedUserId = requestedUser.Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = author.Id;
                if ((int)db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < (int)User.Privileges.Admin)
                {
                    Logger.Warning(author.Id.ToString(), "User tried to use demote command and failed");
                    await ReplyAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }

                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                {
                    if ((int)user.Privilege == (int)User.Privileges.User)
                    {
                        Logger.Info(author.Id.ToString(), $"User <@{author.Id}> isn't a mod.");
                        await ReplyAsync($"<@{author.Id}> isn't a `moddlet`.");

                        return;
                    }

                    user.Privilege = User.Privileges.User;

                    SocketGuildChannel channel = Context.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Moddlet")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).FirstOrDefault()
                    };

                    await channel.Guild.GetUser((ulong)parsedUserId).RemoveRolesAsync(roles);

                    Logger.Info(author.Id.ToString(), $"Demoted <@{requestedUser.Id}> from moderator");
                    await ReplyAsync($"<@{requestedUser.Id}> is no longer `moddlet`!");
                }

                db.SaveChanges();
            }
        }

        [Command("promote", RunMode = RunMode.Async)]
        public async Task PromoteAsync(IUser requestedUser)
        {
            SocketUser author = Context.User;

            // User to ascend
            ulong parsedUserId = requestedUser.Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = author.Id;
                if ((int)db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < (int)User.Privileges.Admin)
                {
                    Logger.Warning(author.Id.ToString(), "User tried to use ascend command and failed");
                    await ReplyAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }

                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                {
                    if ((int)user.Privilege >= (int)User.Privileges.Moderator)
                    {
                        Logger.Info(author.Id.ToString(), $"User <@{author.Id}> already mod or above.");
                        await ReplyAsync($"<@{author.Id}> is already `moddlet` or above.");

                        return;
                    }

                    user.Privilege = User.Privileges.Moderator;

                    SocketGuildChannel channel = Context.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Moddlet")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).FirstOrDefault()
                    };

                    await channel.Guild.GetUser(parsedUserId).AddRolesAsync(roles);

                    Logger.Info(author.Id.ToString(), $"Made <@{requestedUser.Id}> moderator");
                    await ReplyAsync($"SMACK! <@{requestedUser.Id}> has been made `moddlet`!");
                }

                db.SaveChanges();
            }
        }

        [Command("ban", RunMode = RunMode.Async)]
        public async Task BanAsync(IUser requestedUser, [Remainder] string reason = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < User.Privileges.Admin)
                {
                    Logger.Warning(Context.User.Username, "User tried to use ban command and failed");
                    await ReplyAsync($"Looks like someone wants to *get* a ban. Call an admin will ya?");
                    return;
                }

                if (reason == null)
                {
                    await Context.Guild.AddBanAsync(requestedUser, 0, $"Banned by {Context.User.Username}: no reason given");

                    return;
                }

                await Context.Guild.AddBanAsync(requestedUser, 0, $"Banned by {Context.User.Username}: {reason}");
            }
        }

        [Command("kick", RunMode = RunMode.Async)]
        public async Task KickAsync(IUser requestedUser, [Remainder] string reason = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = Context.User.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < User.Privileges.Admin)
                {
                    Logger.Warning(Context.User.Username, "User tried to use ban command and failed");
                    await ReplyAsync($"No can do Jonny boy. You need admin for that.");
                    return;
                }

                await ServerUtilities.KickUserHelper.KickAsync(Context.Channel as SocketTextChannel, requestedUser as SocketGuildUser);
                Logger.Warning(Context.User.Username, $"Kicked {requestedUser.Username} by {Context.User.Username}");
            }
        }

        [Command("purge", RunMode = RunMode.Async)]
        public async Task PurgeAsync()
        {
            SocketUser author = Context.User;

            using (DiscordContext db = new DiscordContext())
            {
                if (db.Users.Where(x => x.DiscordId == author.Id).FirstOrDefault().Privilege < User.Privileges.Admin)
                {
                    Logger.Debug(author.Username, "User attempted pruge command");
                    await ReplyAsync("Do you want to start a riot? ");
                }

                SocketGuildChannel channel = Context.Channel as SocketGuildChannel;
                List<SocketGuildUser> users = channel.Guild.Users.ToList();

                await ReplyAsync("Let the purge begin! :trumpet: ");
                Logger.Debug(author.Username, "Purging the server!");

                DateTime twoWeeksAgo = DateTime.UtcNow.AddDays(-14);

                foreach (SocketGuildUser u in users)
                {
                    User databaseUser = db.Users.Where(x => x.DiscordId == u.Id).FirstOrDefault();

                    if (databaseUser == null)
                    {
                        Logger.Warning("System", $"{u.Username} not registered!");
                        continue;
                    }

                    if (databaseUser.Privilege >= User.Privileges.Moderator)
                    {
                        Logger.Info("System", $"Skipping: {u.Username}, user is moderator or higher.");
                        continue;
                    }

                    if (u.Id == 155149108183695360 || u.Id == UserIds.Luna)
                    {
                        Logger.Info("System", $"Skipping: {u.Username}, bot");
                        continue;
                    }

                    // check if user has messaged in the past 2 weeks. Kick if false
                    if (databaseUser.LastMessage.Subtract(twoWeeksAgo).TotalDays < 0)//&& databaseUser.TutorialFinished == true)
                    {
                        Thread.Sleep(500);
                        Logger.Info("System", $"Purging:  {u.Username} for inactivity.");
                        await KickUserHelper.KickAsync(channel as SocketTextChannel, u);
                    }
                    else if (databaseUser.TutorialFinished == false)
                    {
                        Logger.Verbose("System", $"Skipping: {u.Username}, tutorial not finished.");
                    }
                    else
                    {
                        Logger.Verbose("System", $"Skipping: {u.Username}, active user.");
                    }
                }

                await ReplyAsync("Purging finished. You all, are the lucky few...");
            }
        }


    }
}
