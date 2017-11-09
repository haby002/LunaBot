using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;
using Discord;

namespace LunaBot.Commands
{
    [LunaBotCommand("Xp")]
    class XpCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                User user = db.Users.Where(x => x.DiscordId == userId).SingleOrDefault();

                Task task = new Task(() => SendMessageAsync(message, user));
                task.Start();

                db.Users.Attach(user);
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }

        private async Task SendMessageAsync(SocketMessage message, User user)
        {
            IDMChannel channel = await message.Author.GetOrCreateDMChannelAsync();
            string output = "XP Info:\n";
            output += string.Format("You are currently level {0}\n", user.Level);
            output += string.Format("{0}xp\\{1}xp", user.Xp, user.Level * 100);
            await channel.SendMessageAsync(output);
        }
    }
}
