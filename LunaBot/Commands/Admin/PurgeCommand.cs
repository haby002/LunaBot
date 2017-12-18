using System;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Linq;
using LunaBot.Database;
using System.Threading;
using System.Threading.Tasks;
using LunaBot.ServerUtilities;

namespace LunaBot.Commands
{
    [LunaBotCommand("Purge")]
    class PurgeCommand :BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                if (db.Users.Where(x => x.DiscordId == message.Author.Id).FirstOrDefault().Privilege < User.Privileges.Admin)
                {
                    Logger.Debug(message.Author.Username, "User attempted pruge command");
                    await message.Channel.SendMessageAsync("Do you want to start a riot? ");
                }

                SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                List<SocketGuildUser> users = channel.Guild.Users.ToList();

                await message.Channel.SendMessageAsync("Let the purge begin! :trumpet: ");
                Logger.Debug(message.Author.Username, "Purging the server!");

                DateTime twoWeeksAgo = DateTime.UtcNow.AddDays(-14);

                foreach (SocketGuildUser u in users)
                {
                    User databaseUser = db.Users.Where(x => x.DiscordId == u.Id).FirstOrDefault();

                    if(databaseUser == null)
                    {
                        Logger.Warning("System", $"{u.Username} not registered!");
                        continue;
                    }

                    if(databaseUser.Privilege >= User.Privileges.Moderator)
                    {
                        Logger.Info("System", $"Skipping: {u.Username}, user is moderator or higher.");
                        continue;
                    }

                    if(u.Id == 155149108183695360 || u.Id == UserIds.Luna)
                    {
                        Logger.Info("System", $"Skipping: {u.Username}, bot");
                        continue;
                    }

                    // check if user has messaged in the past 2 weeks. Kick if false
                    if (databaseUser.LastMessage.Subtract(twoWeeksAgo).TotalDays < 0)//&& databaseUser.TutorialFinished == true)
                    {
                        Thread.Sleep(500);
                        Logger.Info("System", $"Purging:  {u.Username} for inactivity.");
                        await KickUserHelper.KickAsync(channel as SocketTextChannel, u);
                    }
                    else if(databaseUser.TutorialFinished == false)
                    {
                        Logger.Verbose("System", $"Skipping: {u.Username}, tutorial not finished.");
                    }
                    else
                    {
                        Logger.Verbose("System", $"Skipping: {u.Username}, active user.");
                    }
                }

                await message.Channel.SendMessageAsync("Purging finished. You all, are the lucky few...");
            }

        }
    }
}
