using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LunaBot.Database;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.Modules
{
    class GetAttributes : ModuleBase<SocketCommandContext>
    {
        [Command("desc", RunMode = RunMode.Async)]
        public async Task GetDescAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(Context.User.Username, $"user <@{userId}> description not found.");
                        await ReplyAsync($"<@{userId}> has no description. *Mysterious...*");

                        return;
                    }

                    Logger.Verbose(Context.User.Username, $"Looking for {userId} description.");
                    await ReplyAsync($"<@{userId}> describes themselve as: {user.Description}");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("d", RunMode = RunMode.Async)]
        public async Task GetDescriptionExtensionAsync(IUser requestedUser = null)
        {
            await GetDescAsync(requestedUser);
        }

        [Command("age", RunMode = RunMode.Async)]
        public async Task GetAgeAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Age <= 0)
                    {
                        if (user.Age == 0)
                            Logger.Warning(Context.User.Username, $"user <@{userId}> age not set.");

                        await ReplyAsync($"<@{userId}> is ageless");

                        return;
                    }

                    Logger.Verbose(Context.User.Username, $"Looking for {userId} description.");
                    await ReplyAsync($"<@{userId}> is {user.Age} years old.");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("a", RunMode = RunMode.Async)]
        public async Task GetAgeExtensionAsync(IUser requestedUser = null)
        {
            await GetAgeAsync(requestedUser);
        }

        [Command("fur", RunMode = RunMode.Async)]
        public async Task GetFurAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(Context.User.Username, $"user <@{userId}> fur not found.");
                        await ReplyAsync($"<@{userId}> has no fur. Maybe they're invisible...");

                        return;
                    }

                    Logger.Verbose(Context.User.Username, $"Looking for {userId} fur.");
                    await ReplyAsync($"<@{userId}> is a {user.Fur}");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("f", RunMode = RunMode.Async)]
        public async Task GetFurExtensionAsync(IUser requestedUser = null)
        {
            await GetFurAsync(requestedUser);
        }

        [Command("lvl", RunMode = RunMode.Async)]
        public async Task GetLvlAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    double percentage = user.Xp / (user.Level * 15);
                    string progressbar = "";

                    for (int i = 0; i < percentage; i++)
                    {
                        progressbar += ("▰");
                    }

                    while (progressbar.Count() < 10)
                    {
                        progressbar += "▱";
                    }

                    Logger.Warning(Context.User.Username, $"looking for user <@{userId}> level.");
                    await ReplyAsync($"<@{userId}> is level {user.Level}\n" +
                        progressbar + " " + (percentage * 10) + "%");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("level", RunMode = RunMode.Async)]
        public async Task GetLvlExtensionAsync(IUser requestedUser = null)
        {
            await GetLvlAsync(requestedUser);
        }

        [Command("xp", RunMode = RunMode.Async)]
        public async Task GetXpAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    double percentage = user.Xp / (user.Level * 15);
                    string progressbar = "";

                    for (int i = 0; i < percentage; i++)
                    {
                        progressbar += ("▰");
                    }

                    while (progressbar.Count() < 10)
                    {
                        progressbar += "▱";
                    }

                    Logger.Warning(Context.User.Username, $"looking for user <@{userId}> xp.");
                    await ReplyAsync($"<@{userId}> has {user.Xp}/{user.Level * 150} xp\n" +
                        progressbar + " " + (percentage * 10) + "%");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("ref", RunMode = RunMode.Async)]
        public async Task GetRefAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(Context.User.Username, $"user <@{userId}> ref not found.");
                        await ReplyAsync($"<@{userId}> has no ref. use this one instead -> :wolf:");

                        return;
                    }

                    Logger.Verbose(Context.User.Username, $"Looking for {userId} ref.");
                    await ReplyAsync($"<@{userId}> ref: {user.Ref}");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("o", RunMode = RunMode.Async)]
        public async Task GetOrientationAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(Context.User.Username, $"looking for user @<{userId}> orientation.");
                    await ReplyAsync($"<@{userId}> is {user.orientation.ToString().ToLower()}");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("orientation", RunMode = RunMode.Async)]
        public async Task GetOrientationExtensionAsync(IUser requestedUser = null)
        {
            await GetOrientationAsync(requestedUser);
        }

        [Command("g", RunMode = RunMode.Async)]
        public async Task GetGenderAsync(IUser requestedUser = null)
        {

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(Context.User.Username, $"looking for user @<{userId}> gender.");
                    await ReplyAsync($"<@{userId}> is {user.Gender.ToString().ToLower()}");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("gender", RunMode = RunMode.Async)]
        public async Task GetGenderExtensionAsync(IUser requestedUser = null)
        {
            await GetGenderAsync(requestedUser);
        }

        [Command("priv", RunMode = RunMode.Async)]
        public async Task GetPrivAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Verbose(Context.User.Username, $"Looking for {userId} privilege.");
                    if (user.Privilege == User.Privileges.Admin || user.Privilege == User.Privileges.Owner)
                    {
                        await ReplyAsync($"<@{userId}> is an `{user.Privilege.ToString()}`");

                        return;
                    }

                    await ReplyAsync($"<@{userId}> is a `{user.Privilege.ToString()}`");

                    return;
                }

                Logger.Verbose(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("snug", RunMode = RunMode.Async)]
        public async Task GetSnugAsync(IUser requestedUser = null)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (requestedUser != null)
                {
                    userId = requestedUser.Id;
                }
                else
                {
                    userId = Context.User.Id;
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Verbose(Context.User.Username, $"Looking for snug counts.");

                    await ReplyAsync($"<@{userId}> has given {user.SnugG} snugs and have recieved {user.SnugR} snugs.");

                    return;
                }

                Logger.Warning(Context.User.Username, $"Failed to find user: {userId}");
                await ReplyAsync($"Failed to find user: `{Context.User.Username}`");

            }
        }

        [Command("s", RunMode = RunMode.Async)]
        public async Task GetSnugExtensionAsync(IUser requestedUser = null)
        {
            await GetSnugAsync(requestedUser);
        }
    }
}
