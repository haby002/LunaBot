using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("Ping")]
    class PingCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            message.Channel.SendMessageAsync("Pong!");
        }
    }
}
