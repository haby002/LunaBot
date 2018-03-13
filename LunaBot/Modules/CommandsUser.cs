using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Modules
{
    class CommandsUser : ModuleBase<SocketCommandContext>
    {
        [Command("ping", RunMode = RunMode.Async)]
        public async Task PingAsync()
        {
            await ReplyAsync(":ping_pong: Pong!");
        }

        [Command("roll", RunMode = RunMode.Async)]
        public async Task RollAsync([Remainder] string parameters)
        {
            string[] rollList = parameters.Split(' ');
            foreach (string p in rollList)
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

        [Command("help", RunMode = RunMode.Async)]
        public async Task HelpAsync([Remainder] string verboseString = null)
        {
            List<string> commands = new List<string>();
            SocketUser author = Context.User;

            using (DiscordContext db = new DiscordContext())
            {
                bool verbose = String.Equals(verboseString, "verbose");
                User user = db.Users.FirstOrDefault(x => x.DiscordId == author.Id);
                commands.Add("My purpose is to allow you to customize a profile as well as providing moderating tools.\n" +
                    "For a more in-depth explaination of these commands type `!help verbose`");

                commands.Add("**User Commands**");

                commands.Add("See your own attributes:\n" +
                    "```?<desc, g, o, age, fur, ref, snug>```");
                commands.Add("See others attributes:\n" +
                    "```?<desc, g, o, age, fur, ref, snug> <user>```");
                commands.Add("Set your attributes:\n" +
                    "```+<desc, g, o, age, fur, ref>```");
                if (verbose)
                    commands.Add("desc = description\n" +
                        "g = gender\n" +
                        "o = orientation\n" +
                        "age = user's age\n" +
                        "fur = fursona\n" +
                        "ref = reference sheet");
                commands.Add("Get Help:\n" +
                    "```!help```");
                commands.Add("Roll:\n" +
                    "```!roll <number>d<size> <number>d<size> ...etc```");
                commands.Add("Snug:\n" +
                    "```!snug <user>```");
                commands.Add("Change SFW and RP modes:\n" +
                    "```+<sfw, monk> <yes, no>```");
                commands.Add("Use an action:\n" +
                    "```!action <action> <user>```");
                if (verbose)
                    commands.Add("Available actions:\n" +
                        "snug\n" +
                        "smooch\n" +
                        "bap\n" +
                        "boop\n");

                if (user.Privilege >= User.Privileges.Moderator)
                {
                    commands.Add("**Moderator Commands**");
                    commands.Add("Set others attributes:\n" +
                        "```!set <user> <attribute> <content>```\n" +
                        "These can be nick, desc, age, gender, orientation, fur, xp, level, ref, or forcetut");
                    commands.Add("Force Tutorial:\n" +
                        "```!forcetut <user>```");
                    commands.Add("Mute/Timeout:\n" +
                        "```!timeout <user> <minutes>```");
                    commands.Add("Ban user:\n" +
                        "```!ban <user> <reason>```");
                    commands.Add("Kick users:\n" +
                        "```!kick <user>```");
                    commands.Add("Warn user:\n" +
                        "```!warn <user>```");
                    commands.Add("Remove warn from user:\n" +
                        "```!removewarn <user> <optional number>```\n" +
                        "The number at the end is optional, otherwise it will remove 1 warn.");
                }

                if (user.Privilege >= User.Privileges.Admin)
                {
                    commands.Add("**Admin Commands**");
                    commands.Add("Promote to Moderator:\n" +
                        "```!promote <user>```");
                    commands.Add("Demote to User:\n" +
                        "```!demote <user>```");
                    commands.Add("Delete intro rooms:\n" +
                        "```!fixrooms```");
                    commands.Add("Purge users:\n" +
                        "```!purge```");
                }

                if (user.Privilege >= User.Privileges.Owner)
                {
                    commands.Add("**Owner Commands**");
                    commands.Add("Ascend to Admin:\n" +
                        "```!ascend <user>```");
                    commands.Add("Descend to Admin:\n" +
                        "```!descend <user>```");
                }
                try
                {
                    await author.SendMessageAsync(string.Join('\n', commands));
                    await ReplyAsync($"<@{author.Id}>, I have sent you your available commands.");
                }
                catch (Exception)
                {
                    await ReplyAsync($"Sorry <@{author.Id}>, you have blocked me from sending you DMs so here are your commands.");
                    await ReplyAsync(string.Join('\n', commands));
                    Logger.Warning(author.Username, "Blocks DMs, Sending commands to server.");
                }
            }
        }

        [Command("snug", RunMode = RunMode.Async)]
        public async Task SnugAsync(IUser requestedUser)
        {
            // return if used in the lobby
            if (Context.Channel.Id == 308306400717832192)
                return;

            using (DiscordContext db = new DiscordContext())
            {
                SocketUser author = Context.User;
                ulong userId = author.Id;
                ulong userId2 = requestedUser.Id;

                Random random = new Random();
                int rand = random.Next(0, 2);

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                User user2 = db.Users.FirstOrDefault(x => x.DiscordId == userId2);

                Logger.Info(author.Username, " is snugging.");

                if (userId == userId2)
                {
                    await ReplyAsync($"<@{userId}> is now snuggling by themselves. A bit lonely but no-one is judging.");
                }
                else
                {
                    user.SnugG = user.SnugG + 1;
                    user2.SnugR = user.SnugR + 1;
                    db.SaveChanges();

                    if (rand == 0)
                    {
                        await ReplyAsync($"<@{userId}> is now snuggling with <@{userId2}>!");
                    }
                    else if (rand == 1)
                    {
                        await ReplyAsync($"Aww, look at <@{userId}> and <@{userId2}> snuggling!");
                    }
                    else if (rand == 2)
                    {
                        await ReplyAsync($"*<@{userId}> grabs <@{userId2}> and they start to snuggle.*");
                    }
                    else
                    {
                        Logger.Warning(author.Username, "Tried to snug with someone and it failed somehow.");
                    }
                }
            }
        }

        [Command("action", RunMode = RunMode.Async)]
        public async Task ActionAsync(string action, IUser requestedUser)
        {
            // return if used in the lobby
            if (Context.Channel.Id == 308306400717832192)
                return;

            SocketUser author = Context.User;
            ulong userId = author.Id;
            ulong userId2;

            action = action.ToLower();

            userId2 = requestedUser.Id;

            Logger.Info(author.Username, " is using an action.");

            if (userId == userId2)
            {
                await ReplyAsync($"<@{userId}>. :facepalm: look, you can't just use an action on yourself ok?");
            }
            else
            {
                if (action == "bap")
                {
                    await ReplyAsync($"<@{userId}> :newspaper2: <@{userId2}>");
                }
                else if (action == "smooch")
                {
                    await ReplyAsync($":heart: <@{userId}> :kissing_heart: <@{userId2}> :heart:");
                }
                else if (action == "boop")
                {
                    await ReplyAsync($"<@{userId}> :point_right: <@{userId2}> *Boop*.");
                }
                else if (action == "punch")
                {
                    await ReplyAsync($"<@{userId}> :right_facing_fist: :boom: <@{userId2}>");
                }
                else
                {
                    await ReplyAsync($"Sorry <@{userId}> that was not an action or it was misspelt...\n" +
                                                            $"Avaliable actions:\n" +
                                                            $"```\n" +
                                                            $"Bap\n" +
                                                            $"Smooch\n" +
                                                            $"Boop\n" +
                                                            $"Punch\n" +
                                                            $"```");
                }
            }
        }
    }
}
