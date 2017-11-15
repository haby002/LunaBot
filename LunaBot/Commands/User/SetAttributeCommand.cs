using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("set_Desc")]
    class SetDescCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(message.Author.Username, $"user <@{userId}> description not found.");
                        message.Channel.SendMessageAsync($"<@{userId}> has no description. *Mysterious...*");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Setting description for {userId} to: ");
                    message.Channel.SendMessageAsync($"<@{userId}> describes themselve as: {user.Description}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_Age")]
    class SetAgeCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Age == 0)
                    {
                        Logger.Warning(message.Author.Username, $"user <@{userId}> age not set.");
                        message.Channel.SendMessageAsync($"<@{userId}> is ageless");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} description.");
                    message.Channel.SendMessageAsync($"<@{userId}> is {user.Age} years old.");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_Lvl")]
    class SetLvlCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"looking for user <@{userId}> level.");
                    message.Channel.SendMessageAsync($"<@{userId}> is level {user.Level}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_xp")]
    class SetXpCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"looking for user <@{userId}> xp.");
                    message.Channel.SendMessageAsync($"<@{userId}> has {user.Xp}xp");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_g")]
    class SetGenderCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"looking for user @<{userId}> gender.");
                    message.Channel.SendMessageAsync($"<@{userId}> is `{user.Gender.ToString()}`");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_o")]
    class SetOrientationCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"looking for user @<{userId}> orientation.");
                    message.Channel.SendMessageAsync($"<@{userId}> is {user.orientation.ToString()}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_Fur")]
    class SetFurCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(message.Author.Username, $"user <@{userId}> fur not found.");
                        message.Channel.SendMessageAsync($"<@{userId}> has no fur. Maybe they're invisible...");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} fur.");
                    message.Channel.SendMessageAsync($"<@{userId}> is a {user.Fur}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_Ref")]
    class SetRefCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    if (user.Description == null)
                    {
                        Logger.Warning(message.Author.Username, $"user <@{userId}> ref not found.");
                        message.Channel.SendMessageAsync($"<@{userId}> has no ref. use this one instead -> :wolf:");

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Looking for {userId} ref.");
                    message.Channel.SendMessageAsync($"<@{userId}> is a {user.Fur}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_priv")]
    class SetPrivilegeCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {

            using (DiscordContext db = new DiscordContext())
            {
                long userId;

                if (message.MentionedUsers.Count > 0)
                {
                    userId = Convert.ToInt64(message.MentionedUsers.First().Id);
                }
                else
                {
                    userId = Convert.ToInt64(parameters[1]);
                }
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Verbose(message.Author.Username, $"Looking for {userId} privilege.");
                    if (user.Privilege == User.Privileges.Admin || user.Privilege == User.Privileges.User)
                    {
                        message.Channel.SendMessageAsync($"<@{userId}> is a `{user.Privilege.ToString()}`");

                        return;
                    }

                    message.Channel.SendMessageAsync($"<@{userId}> is a `{user.Privilege.ToString()}`");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }
}
