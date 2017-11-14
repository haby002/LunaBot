using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("Ascend")]
    class AscendCommand :BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if (parameters.Length != 1)
            {
                Logger.Verbose(message.Author.Username, "Failed ascend command");
                message.Channel.SendMessageAsync("Error: Wrong syntax, try !ascend `user`.");

                return;
            }

            string[] unparsedUserId = parameters[0].Split(new[] { '@', '>' });

            // Check if user attached is correct.
            if (unparsedUserId.Length < 2 || unparsedUserId[0] != "<" || unparsedUserId[1].Length != 18)
            {
                Logger.Verbose(message.Author.Username, "Failed ascend command");
                message.Channel.SendMessageAsync("Error: Command requires an attached `user` to command. Forgot the '@'?");

                return;
            }

            // User to ascend
            long parsedUserId = long.Parse(unparsedUserId[1]);

            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                if ((int)db.Users.Where(x => x.DiscordId == userId).First().Privilege != 3)
                {
                    Logger.Warning(message.Author.Id.ToString(), "User tried to use ascend command and failed");
                    message.Channel.SendMessageAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }
                
                User user = db.Users.Where(x => x.DiscordId == parsedUserId).First();
                {
                    if((int)user.Privilege >= 2)
                    {
                        Logger.Info(message.Author.Id.ToString(), $"User {parameters[0]} already admin.");
                        message.Channel.SendMessageAsync($"{parameters[0]} is already `admin` or above.");

                        return;
                    }
                    user.Privilege = User.Privileges.Admin;

                    SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Hoarder")).First(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).First()
                    };
                    
                    channel.Guild.GetUser((ulong)parsedUserId).AddRolesAsync(roles);

                    Logger.Info(message.Author.Id.ToString(), $"Made {parameters[0]} admin and moderator");
                    message.Channel.SendMessageAsync($"SHAPOW! {parameters[0]} has been made `admin`!");
                }

                db.SaveChanges();
            }
        }
    }
}
