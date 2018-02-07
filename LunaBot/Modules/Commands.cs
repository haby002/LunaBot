using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Modules
{
    class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
        }

        [Command("roll")]
        public async Task RollAsync([Remainder] [Summary("Roll any number and type of dice")] string parameters)
        {
            String[] splitParams = parameters.Split(' ');
            foreach (string p in splitParams)
            {
                string[] rolePts = p.Split('d');
                if (rolePts.Count() != 2)
                {
                    throw new ArgumentException("Invalid roll, roll must be of the form #d#");
                }

                int dice = Convert.ToInt32(rolePts[0]);
                int diceType = Convert.ToInt32(rolePts[1]);
                int sum = 0;
                Random rand = new Random();
                for (int i = 0; i < dice; i++)
                {
                    sum += (rand.Next() % diceType) + 1;
                }

                await ReplyAsync(string.Format("I rolled {0} and got {1}", p, sum));
            }
        }
    }
}
