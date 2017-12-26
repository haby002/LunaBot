using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace LunaBot.Commands
{
    [LunaBotCommand("Action", "A")]
    class ActionCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            ulong userId = message.Author.Id;
            ulong userId2;

            if (message.MentionedUsers.Count > 0) 
            {
                string action = parameters[0];
                action.ToLower();

                userId2 = message.MentionedUsers.FirstOrDefault().Id;

                Logger.Info(message.Author.Username, " is doing something.");

                if(userId == userId2)
                {
                    await message.Channel.SendMessageAsync($"<@{userId}>. :facepalm: look, you can't just use an action on yourself ok?");
                }
                else
                {
                    if (action == "bap")
                    {
                        await message.Channel.SendMessageAsync($"<@{userId}> :newspaper2: <@{userId2}>");
                    }
                    else if (action == "smooch")
                    {
                        await message.Channel.SendMessageAsync($":heart: <@{userId}> :kissing_heart: <@{userId2}> :heart:");
                    }
                    else if (action == "boop")
                    {
                        await message.Channel.SendMessageAsync($"<@{userId}> :point_right: <@{userId2}> *Boop*.");
                    }
                    else if (action == "punch")
                    {
                        await message.Channel.SendMessageAsync($"<@{userId}> :right_facing_fist: :boom: <@{userId2}>");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync($"Sorry <@{userId}> that was not an action or it was misspelt...\n" +
                                                                $"Avaliable actions:\n" +
                                                                $"```\n" +
                                                                $"Bap\n" +
                                                                $"Smooch\n" +
                                                                $"boop\n" +
                                                                $"punch\n" +
                                                                $"```");
                    }
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($"The command goes like this: \n `!Action <Action> <user> \n" +
                                                        $"Avaliable actions:\n" +
                                                        $"```\n" +
                                                        $"Bap\n" +
                                                        $"Smooch\n" +
                                                        $"boop\n" +
                                                        $"punch\n" +
                                                        $"```");
                return;
                
            }
        }
    }
}