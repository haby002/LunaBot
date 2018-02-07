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
    class CommandsOwner : ModuleBase<SocketCommandContext>
    {
        [Command("ascend")]
        public async Task AscendAsync(IUser requestedUser)
        {
            SocketUser author = Context.User;

            // User to ascend
            ulong parsedUserId = requestedUser.Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = author.Id;
                if ((int)db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege != 3)
                {
                    Logger.Warning(author.Id.ToString(), "User tried to use ascend command and failed");
                    await ReplyAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }

                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                {
                    if ((int)user.Privilege >= 2)
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
            }
        }

        [Command("descend")]
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
            }

        }

    }
}
