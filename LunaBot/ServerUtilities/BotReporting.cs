using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.ServerUtilities
{
    class BotReporting
    {
        private SocketChannel reportChannel;

        public BotReporting(SocketChannel rc)
        {
            reportChannel = rc;
        }

        public bool report()
        {
            return true;
        }
    }
}
