using Discord;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using LunaBot.Database;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("Help")]
    class HelpCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            List<string> commands = new List<string>();

            SocketUser author = message.Author;

            using (DiscordContext db = new DiscordContext())
            {
                User user = db.Users.FirstOrDefault(x => x.DiscordId == message.Author.Id);

                commands.Add("**User Commands**");

                commands.Add("See your own attributes:\n" +
                    "```?<desc, g, o, a, f, ref, snug>```");
                commands.Add("See others attributes:\n" +
                    "```?<desc, g, o, a, f, ref, snug> <user>```");
                commands.Add("Set your attributes:\n" +
                    "```+<desc, g, o, a, f, ref>```");
                commands.Add("Get Help:\n" +
                    "```!help```");
                commands.Add("Roll:\n" +
                    "```!roll <number>d<size> <number>d<size> ...etc```");
                commands.Add("Snug:\n" +
                    "```!snug <user>```");
                commands.Add("Change SFW and Monk modes:\n" +
                    "```+<sfw, monk> <yes, no>```");
                commands.Add("Use an action:\n" +
                    "```!action <action> <user>```");

                if (user.Privilege > User.Privileges.User)
                {
                    commands.Add("**Moderator Commands**");
                    commands.Add("Set others attributes:\n" +
                        "```!set <user> <attribute> <content>```");
                    commands.Add("Force Tutorial:\n" +
                        "```!forcetut <user>```");
                }

                if (user.Privilege > User.Privileges.Moderator)
                {
                    commands.Add("**Admin Commands**");
                    commands.Add("Promote to Moderator:\n" +
                        "```!promote <user>```");
                    commands.Add("Demote to User:\n" +
                        "```!demote <user>```");
                    commands.Add("Delete intro rooms:\n" +
                        "```!fixrooms```");
                    commands.Add("Purge users:\n" +
                        "```!purge```");
                }

                if(user.Privilege > User.Privileges.Admin)
                {
                    commands.Add("**Owner Commands**");
                    commands.Add("Ascend to Admin:\n" +
                        "```!ascend <user>```");
                    commands.Add("Descend to Admin:\n" +
                        "```!descend <user>```");
                }

                await author.SendMessageAsync(string.Join('\n', commands));
                await message.Channel.SendMessageAsync($"<@{author.Id}>, I have sent you your available commands.");
            }
        }
    }
}
