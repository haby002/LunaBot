using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("snug", "s")]
    class SnugCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                ulong userId2;

                if (message.MentionedUsers.Count > 0)
                {
                    Random random = new Random();
                    int rand = random.Next(0, 2);

                    userId2 = message.MentionedUsers.FirstOrDefault().Id;

                    User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                    User user2 = db.Users.FirstOrDefault(x => x.DiscordId == userId2);

                    Logger.Info(message.Author.Username, " is pullen a phat snug.");

                    if (userId == userId2)
                    {
                        await message.Channel.SendMessageAsync($"<@{userId}> is now snuggling by themselves. A bit lonely but no-one is judging.");
                    }
                    else
                    {
                        user.SnugG = user.SnugG + 1;
                        user2.SnugR = user.SnugR + 1;
                        db.SaveChanges();

                        if (rand == 0)
                        {
                            await message.Channel.SendMessageAsync($"<@{userId}> is now snuggling with <@{userId2}>!");
                        }
                        else if (rand == 1)
                        {
                            await message.Channel.SendMessageAsync($"Aww, look at <@{userId}> and <@{userId2}> snuggling!");
                        }
                        else if (rand == 2)
                        {
                            await message.Channel.SendMessageAsync($"*<@{userId}> grabs <@{userId2}> and they start to snuggle.*");
                        }
                        else
                        {
                            Logger.Warning(message.Author.Username, "Tried to snug with someone and it failed somehow.");
                        }
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync($"The command goes like this: \n `!Snug <user>`");
                }
            }

        }
    }
}
