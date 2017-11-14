using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("Desc")]
    class DescCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(parameters[1]);
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(message.Author.Username, $"user @<{userId}> description not found.");
                        message.Channel.SendMessageAsync($"<@{userId}> has no description. *Mysterious...*");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} description.");
                    message.Channel.SendMessageAsync($"<@{userId}> describes themselve as: {user.Description}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("Age")]
    class AgeCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(parameters[1]);
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Age == 0)
                    {
                        Logger.Warning(message.Author.Username, $"user @<{userId}> age not set.");
                        message.Channel.SendMessageAsync($"<@{userId}> is ageless");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} description.");
                    message.Channel.SendMessageAsync($"<@{userId}> is {user.Age} years old.");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }
}
