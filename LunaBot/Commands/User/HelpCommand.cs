using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;
using System.Reflection;

namespace LunaBot.Commands
{
    [LunaBotCommand("Help")]
    class HelpCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            //IDictionary<string, BaseCommand> commandDictionary = new Dictionary<string, BaseCommand>();

            List<string> commands = new List<string>();
            
            using (DiscordContext db = new DiscordContext())
            {
                User user = db.Users.FirstOrDefault(x => x.DiscordId == (long)message.Author.Id);

                //Type[] commands = Assembly.GetExecutingAssembly().GetTypes().Where(t => String.Equals(t.Namespace, "LunaBot.Commands", StringComparison.Ordinal)).ToArray();
                //commands = commands.Where(x => x.GetCustomAttributes(typeof(LunaBotCommandAttribute)).Any()).ToArray();

                //foreach (Type command in commands)
                //{
                //    LunaBotCommandAttribute commandAttribute = command.GetCustomAttribute(typeof(LunaBotCommandAttribute)) as LunaBotCommandAttribute;
                //    commandDictionary[commandAttribute.Name] = Activator.CreateInstance(command) as BaseCommand;
                //}

                commands.Add("**User Commands**");

                commands.Add("See your own attributes:\n" +
                    "```?<desc, g, o, a, f, ref>```");
                commands.Add("See others attributes:\n" +
                    "```?<desc, g, o, a, f, ref> <user>```");
                commands.Add("Set your attributes:\n" +
                    "```+<desc, g, o, a, f, ref>```");
                commands.Add("Get Help:\n" +
                    "```!help```");
                commands.Add("Roll:\n" +
                    "```!roll <number>d<size> <number>d<size> ...etc```");

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

                message.Channel.SendMessageAsync(string.Join('\n', commands));
            }
        }
    }
}
