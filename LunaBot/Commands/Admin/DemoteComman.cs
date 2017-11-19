using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaBot.Commands
{
    [LunaBotCommand("Demote")]
    class DemoteCommand :BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if (parameters.Length != 1)
            {
                Logger.Verbose(message.Author.Username, "Failed demote command");
                message.Channel.SendMessageAsync("Error: Wrong syntax, try !demote `user`.");

                return;
            }
            

            // Check if user attached is correct.
            if (message.MentionedUsers.Count == 0)
            {
                Logger.Verbose(message.Author.Username, "Failed demote command");
                message.Channel.SendMessageAsync("Error: Command requires an attached `user` to command. Forgot the '@'?");

                return;
            }

            // User to demote
            long parsedUserId = (long)message.MentionedUsers.First().Id;

            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                if ((int)db.Users.Where(x => x.DiscordId == userId).First().Privilege < (int)User.Privileges.Admin)
                {
                    Logger.Warning(message.Author.Id.ToString(), "User tried to use demote command and failed");
                    message.Channel.SendMessageAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }
                
                User user = db.Users.Where(x => x.DiscordId == parsedUserId).First();
                {
                    if((int)user.Privilege == (int)User.Privileges.User)
                    {
                        Logger.Info(message.Author.Id.ToString(), $"User {parameters[0]} isn't a mod.");
                        message.Channel.SendMessageAsync($"{parameters[0]} isn't a `moddlet`.");

                        return;
                    }

                    user.Privilege = User.Privileges.User;

                    SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Moddlet")).First(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).First()
                    };
                    
                    channel.Guild.GetUser((ulong)parsedUserId).RemoveRolesAsync(roles);

                    Logger.Info(message.Author.Id.ToString(), $"Demoted {parameters[0]} from moderator");
                    message.Channel.SendMessageAsync($"{parameters[0]} is no longer `moddlet`!");
                }

                db.SaveChanges();
            }
        }
    }
}
