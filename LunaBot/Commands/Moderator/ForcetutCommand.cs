using Discord.WebSocket;
using LunaBot.Database;
using System.Linq;

namespace LunaBot.Commands.Moderator
{
    [LunaBotCommand("forcetut")]
    class GetCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                User databaseUser = db.Users.Where(x => x.DiscordId == 0).First();
            }
        }
    }
}
