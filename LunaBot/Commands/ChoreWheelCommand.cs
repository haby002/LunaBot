using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Discord;

namespace LunaBot.Commands
{
    [LunaBotCommand("ChoreWheel")]
    class ChoreWheelCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            IList<IUser> users = new List<IUser>();
            IList<string> chores = new List<string>()
            {
                "Cook Dinner",
                "Clean From Dinner",
                "Keep Tim In Check",
                "Clean Gutters",
                "Think Of More Chores"
            };
            message.Channel.GetUsersAsync().ForEachAsync(x =>
            {
                foreach(IUser user in x)
                {
                    users.Add(user);
                }
            });

            users = users.Where(x => x.Id != 289924556351733760).ToList();

            Shuffle(users);

            string output = "I HAVE DECIDED ON CHORES:";
            for(int i = 0; i < chores.Count; i++)
            {
                string chore = chores[i];
                IUser persion = users[i % users.Count()];
                output += $"\n{chore}: {persion.Username}";
            }

            message.Channel.SendMessageAsync(output);
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            Random rng = new Random();  
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
