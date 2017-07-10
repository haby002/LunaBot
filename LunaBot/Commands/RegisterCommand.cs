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
                if (db.Users.Where(x => x.DiscordId == userId).Count() != 0)
                {
                    Logger.Verbose(message.Author.Username, "User already registered");
                    message.Channel.SendMessageAsync("You're already registered you goon");

                    return;
                }

                Logger.Verbose(message.Author.Username, "Creating User Data");
                message.Channel.SendMessageAsync("Creating User Data");
                
                User newUser = db.Users.Create(); 
                newUser.DiscordId = userId;
                db.Users.Add(newUser);
                var list = db.Users.ToList();
                db.SaveChanges();

                Logger.Verbose(message.Author.Username, "Created User");
                message.Channel.SendMessageAsync("Created User");

                Logger.Verbose("",newUser.ID.ToString());
            }
        }
    }
}
