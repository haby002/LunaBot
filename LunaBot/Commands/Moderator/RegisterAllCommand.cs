using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaBot.Commands.Moderator
{
    [LunaBotCommand("RegisterAll")]
    class RegisterAllCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                if (db.Users.Where(x => x.DiscordId == userId).First().Privilege == 0)
                {
                    Logger.Warning(message.Author.Username, "Failed RegisterAll command");
                    message.Channel.SendMessageAsync("You're not a moderator, go away.");

                    return;
                }
                
                Logger.Verbose(message.Author.Username, "Fixing Registrations");
                message.Channel.SendMessageAsync("Fixing registrations...");

                SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                List<SocketGuildUser> users = channel.Guild.Users.ToList();

                foreach(SocketGuildUser u in users)
                {
                    if(db.Users.Where(x => x.DiscordId == (long)u.Id).Count() == 0)
                    {
                        Logger.Verbose(message.Author.Username, $"Creating User Data for {u.Username}");

                        User newUser = new User();
                        newUser.DiscordId = userId;
                        newUser.Level = 1;
                        newUser.Privilege = 0;
                        newUser.TutorialFinished = false;
                        newUser.Gender = User.Genders.None;
                        db.Users.Add(newUser);
                        var list = db.Users.ToList();

                        Logger.Verbose(message.Author.Username, $"Created User {newUser.ID.ToString()}");

                    }
                }

                db.SaveChanges();
            }
        }
    }
}
