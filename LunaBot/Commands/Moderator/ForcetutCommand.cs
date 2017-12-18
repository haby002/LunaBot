using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Database;
using LunaBot.ServerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("forcetut")]
    class ForceTutCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if (parameters.Length != 1)
            {
                Logger.Verbose(message.Author.Username, "Failed forcetut command");
                await message.Channel.SendMessageAsync("Error: Wrong syntax, try !forcetut `user`.");

                return;
            }
            
            // Check if user attached is correct.
            if (message.MentionedUsers.Count() == 0)
            {
                Logger.Verbose(message.Author.Username, "Failed forcetut command");
                await message.Channel.SendMessageAsync("Error: Command requires an attached `user` to command.\n" +
                    "Dropped this '@'?");

                return;
            }

            // User to forcetut
            ulong parsedUserId = message.MentionedUsers.FirstOrDefault().Id;

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                SocketGuildUser discordUser = message.MentionedUsers.FirstOrDefault() as SocketGuildUser;

                if (db.Users.Where(x => x.DiscordId == userId).FirstOrDefault().Privilege == User.Privileges.User)
                {
                    Logger.Warning(message.Author.Id.ToString(), "User tried to use forcetut command and failed");
                    await message.Channel.SendMessageAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }

                RegisterCommand registerCommand = new RegisterCommand();
                User user = db.Users.Where(x => x.DiscordId == parsedUserId).FirstOrDefault();

                //Reset database entry for user
                user.ResetUser();

                // Register user in database
                registerCommand.manualRegister(discordUser);
                
                SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                SocketRole role = guildRoles.Where(x => x.Name.Equals("Newbie")).FirstOrDefault();

                await channel.Guild.GetUser((ulong)parsedUserId).AddRoleAsync(role);

                // Creat intro room
                RestTextChannel introRoom = await channel.Guild.CreateTextChannelAsync($"intro-{parsedUserId}");

                await Task.Run(async () =>
                {
                    // Make room only visible to new user, staff, and Luna
                    await introRoom.AddPermissionOverwriteAsync(discordUser, Permissions.userPerm);

                    // Start interaction with user. Sleeps are for humanizing the bot.
                    await introRoom.SendMessageAsync("Welcome to the server! Lets get you settled, alright?");
                    Thread.Sleep(1000);
                    await introRoom.SendMessageAsync("Firstly, what should we call you?");
                }).ConfigureAwait(false);

                db.SaveChanges();
            }
        }
    }
}
