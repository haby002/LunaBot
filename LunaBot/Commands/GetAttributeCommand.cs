using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("desc")]
    class DescCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                User user = db.Users.FirstOrDefault(x => x.ID == userId);
                if (user != null)
                {
                    user.Description = parameters[0];
                    db.SaveChanges();
                    Logger.Verbose(message.Author.Username, $"Updated description for {userId}");
                    message.Channel.SendMessageAsync($"Updated description for {userId}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: {userId}");

            }
        }
    }
}
