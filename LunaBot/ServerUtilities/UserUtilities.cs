using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Linq;

namespace LunaBot.ServerUtilities
{
    internal static class UserUtilities
    {
        public static bool manualRegister(SocketGuildUser user)
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
                newUser.Nickname = "";
                newUser.DiscordId = userId;
                newUser.Level = 1;
                newUser.LastMessage = DateTime.UtcNow;
                newUser.Privilege = 0;
                newUser.TutorialFinished = false;
                newUser.Gender = User.Genders.None;
                newUser.SnugG = 0;
                newUser.SnugR = 0;
                db.Users.Add(newUser);

                db.SaveChanges();

                Logger.Verbose("System", $"Created user {user.Username} successfully");

                return true;
            }
        }
    }
}
