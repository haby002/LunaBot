using Discord.Rest;
using Discord.WebSocket;
using LunaBot.Database;
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
        public override void Process(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if (parameters.Length != 1)
            {
                Logger.Verbose(message.Author.Username, "Failed forcetut command");
                message.Channel.SendMessageAsync("Error: Wrong syntax, try !forcetut `user`.");

                return;
            }

            string[] unparsedUserId = parameters[0].Split(new[] { '@', '>' });

            // Check if user attached is correct.
            if (unparsedUserId.Length < 2 || unparsedUserId[0] != "<" || unparsedUserId[1].Length != 18)
            {
                Logger.Verbose(message.Author.Username, "Failed forcetut command");
                message.Channel.SendMessageAsync("Error: Command requires an attached `user` to command.\n" +
                    "Dropped this '@'?");

                return;
            }

            // User to forcetut
            long parsedUserId = long.Parse(unparsedUserId[1]);

            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                if (db.Users.Where(x => x.DiscordId == userId).First().Privilege == User.Privileges.User)
                {
                    Logger.Warning(message.Author.Id.ToString(), "User tried to use forcetut command and failed");
                    message.Channel.SendMessageAsync($"Nice try. Dont want me calling your parents, right?");
                    return;
                }

                User user = db.Users.Where(x => x.DiscordId == parsedUserId).First();
                {
                    user.TutorialFinished = false;

                    SocketGuildChannel channel = message.Channel as SocketGuildChannel;
                    IReadOnlyCollection<SocketRole> guildRoles = channel.Guild.Roles;

                    SocketRole role = guildRoles.Where(x => x.Name.Equals("newbie")).First();

                    channel.Guild.GetUser((ulong)parsedUserId).AddRoleAsync(role);

                    // Creat intro room
                    RestTextChannel introRoom = channel.Guild.CreateTextChannelAsync($"intro-{parsedUserId}").Result;

                    // Make room only visible to new user and admins
                    introRoom.AddPermissionOverwriteAsync(message.MentionedUsers.First(), Engine.userPerm);
                    introRoom.AddPermissionOverwriteAsync(guildRoles.Where(x => x.Name.Equals("@everyone")).First(), Engine.removeAllPerm);

                    // Register user in database
                    RegisterCommand registerCommand = new RegisterCommand();
                    bool register = registerCommand.manualRegister(message.Author as SocketGuildUser);

                    // Start interaction with user. Sleeps are for humanizing the bot.
                    introRoom.SendMessageAsync("Welcome to the server! Lets get you settled, alright?");
                    Thread.Sleep(2000);
                    introRoom.SendMessageAsync("Firstly, what should we call you?");
                }
            }
        }
    }
}
