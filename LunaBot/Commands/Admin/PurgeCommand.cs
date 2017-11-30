using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Linq;
using LunaBot.Database;
using Discord;
using System.Threading;

namespace LunaBot.Commands
{
    [LunaBotCommand("Purge")]
    class PurgeCommand :BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                if (db.Users.Where(x => x.DiscordId == message.Author.Id).FirstOrDefault().Privilege < User.Privileges.Admin)
                {
                    Logger.Debug(message.Author.Username, "User attempted pruge command");
                    message.Channel.SendMessageAsync("Do you want to start a riot? ");
                }
                SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                List<SocketGuildUser> users = channel.Guild.Users.ToList();

                message.Channel.SendMessageAsync("Let the purge begin! :trumpet: ");
                Logger.Debug(message.Author.Username, "Purging the server!");

                DateTime twoWeeksAgo = DateTime.UtcNow.AddDays(-14);

                foreach (SocketGuildUser u in users)
                {
                    User databaseUser = db.Users.Where(x => x.DiscordId == u.Id).FirstOrDefault();
                    // check if user has messaged in the past 2 weeks. Kick if false
                    if (databaseUser.LastMessage.Subtract(twoWeeksAgo).TotalDays < 0)
                    {
                        Thread.Sleep(500);
                        ServerUtilities.KickUserHelper.kick(channel as SocketTextChannel, u);
                    }
                }
            }

        }
    }
}
