using System.Threading.Tasks;
using Discord.WebSocket;
using System.Threading;

namespace LunaBot.Commands
{
    [LunaBotCommand("Ping")]
    class PingCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            await message.Channel.SendMessageAsync("Sleeping...");
            Thread.Sleep(10000);
            await message.Channel.SendMessageAsync("Wha-? oh! Pong!");
        }
    }
}
