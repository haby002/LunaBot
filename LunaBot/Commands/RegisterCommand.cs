using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

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
                if (db.Users.Any(x => x.ID == userId))
                {
                    Logger.Verbose(message.Author.Username, "User already registered");
                    message.Channel.SendMessageAsync("You're already registered you goon");
                }

                Logger.Verbose(message.Author.Username, "Creating User Data");
                message.Channel.SendMessageAsync("Creating User Data");
                User newUser = new User();
                newUser.ID = userId;
                db.Users.Add(newUser);
                db.SaveChanges();
            }
        }
    }
}
