using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("Descend")]
    class DescendCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if (parameters.Length != 1)
            {
                Logger.Verbose(message.Author.Username, "Failed descend command");
                message.Channel.SendMessageAsync("Error: Wrong syntax, try !descend `user`.");

                return;
            }

            string[] unparsedUserId = parameters[0].Split(new[] { '@', '>' });

            // Check if user attached is correct.
            if (unparsedUserId.Length < 2 || unparsedUserId[0] != "<" || unparsedUserId[1].Length != 18)
            {
                Logger.Verbose(message.Author.Username, "Failed descend command");
                message.Channel.SendMessageAsync("Error: Command requires an attached `user` to command. Forgot the '@'?");

                return;
            }

            // User to descend
            long parsedUserId = long.Parse(unparsedUserId[1]);

            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                if (db.Users.Where(x => x.DiscordId == userId).First().Privilege != User.Privileges.Owner)
                {
                    Logger.Warning(message.Author.Id.ToString(), "User tried to use descend command and failed");
                    message.Channel.SendMessageAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }

                User user = db.Users.Where(x => x.DiscordId == parsedUserId).First();
                {
                    if (user.Privilege == User.Privileges.User)
                    {
                        Logger.Info(message.Author.Id.ToString(), $"User {parameters[0]} not admin.");
                        message.Channel.SendMessageAsync($"{parameters[0]} is not an `admin` or `moderator`.");
                    }
                    else if(user.Privilege == User.Privileges.Moderator)
                    {
                        Logger.Info(message.Author.Id.ToString(), $"Removed moderator from {parameters[0]}");
                        message.Channel.SendMessageAsync($"{parameters[0]} is no longer `moderator`");
                    }
                    else
                    {
                        Logger.Info(message.Author.Id.ToString(), $"Removed admin  and moderator from {parameters[0]}");
                        message.Channel.SendMessageAsync($"{parameters[0]} is no longer `admin` or `moderator`");
                    }

                    user.Privilege = User.Privileges.User;

                    SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Hoarder")).First(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).First(),
                        guildRoles.Where(x => x.Name.Equals("Moddlet")).First()
                    };

                    channel.Guild.GetUser((ulong)parsedUserId).RemoveRolesAsync(roles);
                }

                db.SaveChanges();
            }
        }
    }
}
