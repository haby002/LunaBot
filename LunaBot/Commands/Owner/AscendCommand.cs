//DO NOT DELETE
//I WILL TAKE CODE FROM HERE LATER
/*
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("Ascend")]
    class AscendCommand :BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if (parameters.Length != 1)
            {
                Logger.Verbose(message.Author.Username, "Failed ascend command");
                await message.Channel.SendMessageAsync("Error: Wrong syntax, try !ascend `user`.");

                return;
            }
            

            // Check if user attached is correct.
            if (message.MentionedUsers.Count == 0)
            {
                Logger.Verbose(message.Author.Username, "Failed ascend command");
                await message.Channel.SendMessageAsync("Error: Command requires an attached `user` to command. Forgot the '@'?");

                return;
            }

            // User to ascend
            ulong parsedUserId = message.MentionedUsers.FirstOrDefault().Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                if ((int)db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege != 3)
                {
                    Logger.Warning(message.Author.Id.ToString(), "User tried to use ascend command and failed");
                    await message.Channel.SendMessageAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }
                
                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                {
                    if((int)user.Privilege >= 2)
                    {
                        Logger.Info(message.Author.Id.ToString(), $"User {parameters[0]} already admin.");
                        await message.Channel.SendMessageAsync($"{parameters[0]} is already `admin` or above.");

                        return;
                    }
                    user.Privilege = User.Privileges.Admin;

                    SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Hoarder")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).FirstOrDefault()
                    };

                    await channel.Guild.GetUser(parsedUserId).AddRolesAsync(roles);

                    Logger.Info(message.Author.Id.ToString(), $"Made {parameters[0]} admin and moderator");
                    await message.Channel.SendMessageAsync($"SHAPOW! {parameters[0]} has been made `admin`!");
                }

                db.SaveChanges();
            }
        }
    }
}*/
