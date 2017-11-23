using Discord.WebSocket;
using LunaBot.Database;
using LunaBot.ServerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaBot.Commands
{
    [LunaBotCommand("Timeout")]
    class TimeoutAllCommand : BaseCommand
    {
        public async override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege == 0)
                {
                    Logger.Warning(message.Author.Username, "Failed timout command. Not enough privileges.");
                    await message.Channel.SendMessageAsync("You're not a moderator, go away.");

                    return;
                }

                // Sanity check
                if(message.MentionedUsers.Count == 0)
                {
                    Logger.Warning(message.Author.Username, "Failed timout command. No mentioned user.");
                    await message.Channel.SendMessageAsync("No mentioned user. !timeout <user> <time>");

                    return;
                }

                if(parameters.Length < 2)
                {
                    Logger.Warning(message.Author.Username, "Failed timout command. Time given.");
                    await message.Channel.SendMessageAsync("Please specify an amount of time. !timeout <user> <time>");

                    return;
                }

                if (!int.TryParse(parameters[1], out int seconds))
                {
                    Logger.Warning(message.Author.Username, "Failed timout command. Time for timout failed.");
                    await message.Channel.SendMessageAsync("Time requested not a number. !timeout <user> <time>");

                    return;
                }

                MuteUserHelper.mute(message.Channel as SocketTextChannel, message.MentionedUsers.FirstOrDefault() as SocketGuildUser, seconds);

            }
        }

    }
}
