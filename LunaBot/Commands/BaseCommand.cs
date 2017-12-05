using Discord.WebSocket;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    public abstract class BaseCommand
    {
        public abstract Task Process(SocketMessage message, string[] parameters);
    }
}
