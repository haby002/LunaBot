using System;
using System.Linq;
using Discord.WebSocket;
using LunaBot.Database;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    class RegisterCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                if (db.Users.Where(x => x.DiscordId == userId).Count() != 0)
                {
                    Logger.Verbose(message.Author.Username, "User already registered");
                    await message.Channel.SendMessageAsync("You're already registered you goon.");

                    return;
                }

                Logger.Verbose(message.Author.Username, "Creating User Data");
                await message.Channel.SendMessageAsync("Creating User Data");

                User newUser = new User(); 
                newUser.DiscordId = userId;
                newUser.Level = 1;
                newUser.LastMessage = DateTime.UtcNow;
                newUser.Privilege = 0;
                newUser.TutorialFinished = false;
                newUser.Gender = User.Genders.None;
                db.Users.Add(newUser);
                db.SaveChanges();

                Logger.Verbose(message.Author.Username, "Created User");
                await message.Channel.SendMessageAsync("Created User");

                Logger.Verbose("",newUser.ID.ToString());
            }
        }

        public bool manualRegister(SocketGuildUser user)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = user.Id;
                if (db.Users.Where(x => x.DiscordId == userId).Count() != 0)
                {
                    Logger.Verbose("System", $"User {user.Username} already registered");

                    return false;
                }

                Logger.Verbose("System", $"Creating User {user.Username} Data");

                User newUser = new User();
                newUser.DiscordId = userId;
                newUser.Level = 1;
                newUser.LastMessage = DateTime.UtcNow;
                newUser.Privilege = 0;
                newUser.TutorialFinished = false;
                newUser.Gender = User.Genders.None;
                db.Users.Add(newUser);

                db.SaveChanges();
                
                Logger.Verbose("System", $"Created user {user.Username} successfully");

                return true;
            }
        }
    }
}
