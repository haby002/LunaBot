using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;
using System.Data.Entity;

namespace LunaBot.Commands
{
    [LunaBotCommand("Register")]
    class RegisterCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                if (db.Users.FirstOrDefault<User>(userId))
                {
                    Logger.Verbose(message.Author.Username, "User already registered");
                    message.Channel.SendMessageAsync("You're already registered you goon");

                    return;
                }

                Logger.Verbose(message.Author.Username, "Creating User Data");
                message.Channel.SendMessageAsync("Creating User Data");

                User newUser = new User();
                newUser.ID = userId;
                db.Users.Attach(newUser);
                db.Entry(newUser).State = EntityState.Added;
                db.Users.Add(newUser);
                db.SaveChanges();

                Logger.Verbose(message.Author.Username, "Created User");
                message.Channel.SendMessageAsync("Created User");
            }
        }
    }
}
