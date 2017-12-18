using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("Promote")]
    class PromoteCommand :BaseCommand
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
                if ((int)db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < (int)User.Privileges.Admin)
                {
                    Logger.Warning(message.Author.Id.ToString(), "User tried to use ascend command and failed");
                    await message.Channel.SendMessageAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }
                
                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                {
                    if((int)user.Privilege >= (int)User.Privileges.Moderator)
                    {
                        Logger.Info(message.Author.Id.ToString(), $"User {parameters[0]} already mod or above.");
                        await message.Channel.SendMessageAsync($"{parameters[0]} is already `moddlet` or above.");

                        return;
                    }

                    user.Privilege = User.Privileges.Moderator;

                    SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Moddlet")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).FirstOrDefault()
                    };

                    await channel.Guild.GetUser(parsedUserId).AddRolesAsync(roles);

                    Logger.Info(message.Author.Id.ToString(), $"Made {parameters[0]} moderator");
                    await message.Channel.SendMessageAsync($"SMACK! {parameters[0]} has been made `moddlet`!");
                }

                db.SaveChanges();
            }
        }
    }
}
