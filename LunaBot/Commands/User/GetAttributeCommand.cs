using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("get_Desc", "get_D")]
    class GetDescCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {
            
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(message.Author.Username, $"user <@{userId}> description not found.");
                        await message.Channel.SendMessageAsync($"<@{userId}> has no description. *Mysterious...*");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} description.");
                    await message.Channel.SendMessageAsync($"<@{userId}> describes themselve as: {user.Description}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("get_a", "get_age")]
    class GetAgeCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Age == 0)
                    {
                        Logger.Warning(message.Author.Username, $"user <@{userId}> age not set.");
                        await message.Channel.SendMessageAsync($"<@{userId}> is ageless");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} description.");
                    await message.Channel.SendMessageAsync($"<@{userId}> is {user.Age} years old.");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("get_Lvl", "get_level")]
    class GetLvlCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
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

                    Logger.Warning(message.Author.Username, $"looking for user <@{userId}> level.");
                    await message.Channel.SendMessageAsync($"<@{userId}> is level {user.Level}\n" +
                        progressbar + " " + (percentage * 10) + "%");
                    
                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("get_xp")]
    class GetXpCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    double percentage = user.Xp / (user.Level * 15);
                    string progressbar = "";

                    for(int i = 0; i < percentage; i++)
                    {
                        progressbar += ("▰");
                    }

                    while (progressbar.Count() < 10)
                    {
                        progressbar += "▱";
                    }

                    Logger.Warning(message.Author.Username, $"looking for user <@{userId}> xp.");
                    await message.Channel.SendMessageAsync($"<@{userId}> has {user.Xp}/{user.Level * 150} xp\n" +
                        progressbar + " " + (percentage * 10) + "%");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("get_G", "get_Gender")]
    class GetGenderCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"looking for user @<{userId}> gender.");
                    await message.Channel.SendMessageAsync($"<@{userId}> is {user.Gender.ToString().ToLower()}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("get_o", "get_Orientation")]
    class GetOrientationCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"looking for user @<{userId}> orientation.");
                    await message.Channel.SendMessageAsync($"<@{userId}> is {user.orientation.ToString().ToLower()}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("get_f", "get_fur")]
    class GetFurCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(message.Author.Username, $"user <@{userId}> fur not found.");
                        await message.Channel.SendMessageAsync($"<@{userId}> has no fur. Maybe they're invisible...");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} fur.");
                    await message.Channel.SendMessageAsync($"<@{userId}> is a {user.Fur}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("get_Ref")]
    class GetRefCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(message.Author.Username, $"user <@{userId}> ref not found.");
                        await message.Channel.SendMessageAsync($"<@{userId}> has no ref. use this one instead -> :wolf:");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} ref.");
                    await message.Channel.SendMessageAsync($"<@{userId}> ref: {user.Ref}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("get_priv")]
    class GetPrivilegeCommand : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = message.MentionedUsers.FirstOrDefault().Id;
                }
                else
                {
                    userId = ulong.Parse(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Verbose(message.Author.Username, $"Looking for {userId} privilege.");
                    if(user.Privilege == User.Privileges.Admin || user.Privilege == User.Privileges.Owner)
                    {
                        await message.Channel.SendMessageAsync($"<@{userId}> is an `{user.Privilege.ToString()}`");

                        return;
                    }

                    await message.Channel.SendMessageAsync($"<@{userId}> is a `{user.Privilege.ToString()}`");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }
}
