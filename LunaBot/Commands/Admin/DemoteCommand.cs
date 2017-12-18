using Discord.WebSocket;
using LunaBot.Database;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("Demote")]
    class DemoteCommand :BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if (parameters.Length != 1)
            {
                Logger.Verbose(message.Author.Username, "Failed demote command");
                await message.Channel.SendMessageAsync("Error: Wrong syntax, try !demote `user`.");

                return;
            }
            

            // Check if user attached is correct.
            if (message.MentionedUsers.Count == 0)
            {
                Logger.Verbose(message.Author.Username, "Failed demote command");
                await message.Channel.SendMessageAsync("Error: Command requires an attached `user` to command. Forgot the '@'?");

                return;
            }

            // User to demote
            ulong parsedUserId = message.MentionedUsers.FirstOrDefault().Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                if ((int)db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege < (int)User.Privileges.Admin)
                {
                    Logger.Warning(message.Author.Id.ToString(), "User tried to use demote command and failed");
                    await message.Channel.SendMessageAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }
                
                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();
                {
                    if((int)user.Privilege == (int)User.Privileges.User)
                    {
                        Logger.Info(message.Author.Id.ToString(), $"User {parameters[0]} isn't a mod.");
                        await message.Channel.SendMessageAsync($"{parameters[0]} isn't a `moddlet`.");

                        return;
                    }

                    user.Privilege = User.Privileges.User;

                    SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    List<SocketRole> roles = new List<SocketRole>()
                    {
                        guildRoles.Where(x => x.Name.Equals("Moddlet")).FirstOrDefault(),
                        guildRoles.Where(x => x.Name.Equals("Staff")).FirstOrDefault()
                    };

                    await channel.Guild.GetUser((ulong)parsedUserId).RemoveRolesAsync(roles);

                    Logger.Info(message.Author.Id.ToString(), $"Demoted {parameters[0]} from moderator");
                    await message.Channel.SendMessageAsync($"{parameters[0]} is no longer `moddlet`!");
                }

                db.SaveChanges();
            }
        }
    }
}
