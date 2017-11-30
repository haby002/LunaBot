using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaBot.Commands
{
    [LunaBotCommand("RegisterAll")]
    class RegisterAllCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege == 0)
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
                    if(db.Users.Where(x => x.DiscordId == u.Id).Count() == 0)
                    {
                        Logger.Verbose(message.Author.Username, $"Creating User Data for {u.Username}");

                        User newUser = new User();
                        newUser.DiscordId = u.Id;
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
