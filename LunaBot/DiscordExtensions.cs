using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot
{
    public static class DiscordExtensions
    {
        /// <summary>
        /// Log the socket message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="sev">The severity</param>
        /// <returns>the logging task</returns>
        public static Task Log(this SocketMessage message, LogSeverity sev = LogSeverity.Verbose)
        {
            return Logger.Log(new LogMessage(sev, message.Author.ToString(), message.Content));
        }
    }
}
