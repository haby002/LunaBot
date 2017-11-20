using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Linq;
using LunaBot.Database;
using Discord;

namespace LunaBot.Commands
{
    [LunaBotCommand("Purge")]
    class PurgeCommand :BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                if (db.Users.Where(x => x.DiscordId == (long)message.Author.Id).FirstOrDefault().Privilege < User.Privileges.Admin)
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
                    User databaseUser = db.Users.Where(x => x.DiscordId == (long)u.Id).FirstOrDefault();
                    // check if user has messaged in the past 2 weeks. Kick if false
                    if (databaseUser.LastMessage.Subtract(twoWeeksAgo).TotalDays < 0)
                    {
                        Logger.Info("System", $"Kicking {u.Username}");
                        
                        IDMChannel dm = u.GetOrCreateDMChannelAsync().Result;
                        dm.SendMessageAsync("You have been kicked from the server from inactivity.\n" +
                            "You can join again but once you get kicked 3 times you are banned.\n" +
                            "Hint: Prevent getting kicked by being part of the community.\n" +
                            "https://discord.gg/J4c8wKg");

                        u.KickAsync("Purged for inactivity");
                        message.Channel.SendMessageAsync($"Critical hit! {u.Username} bit the dust.\n");
                    }
                }
            }

        }
    }
}
