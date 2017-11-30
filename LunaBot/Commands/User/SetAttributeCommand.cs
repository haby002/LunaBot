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
                ulong userId = message.Author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);

                if (user != null)
                {
                    Logger.Verbose(message.Author.Username, $"Setting description for {userId} to: {parameters[0]}");

                    user.Description = parameters[0];
                    db.SaveChanges();

                    message.Channel.SendMessageAsync($"Changed <@{userId}>'s description to: {user.Description}");

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
            if(!int.TryParse(parameters[0], out int age))
            {
                message.Channel.SendMessageAsync("The numbers Mason, what are the numbers?!");

                return;
            }
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Verbose(message.Author.Username, $"Setting {userId} description to {age}");

                    user.Age = age;
                    db.SaveChanges();

                    message.Channel.SendMessageAsync($"Changed <@{userId}> age to `{user.Age}` years old.");

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
            User.Genders gender = Utilities.StringToGender(parameters[0]);
            if (gender == User.Genders.None)
            {
                message.Channel.SendMessageAsync("Couldn't understand that gender... it can either be\n" +
                    "```\n" +
                    "- Male\n" +
                    "- Female\n" +
                    "- Trans-Female\n" +
                    "- Transe-Male\n" +
                    "- Other\n" +
                    "```");

                return;
            }

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"Setting @<{userId}>'s gender to {gender.ToString()}.");

                    // Adding role to user
                    SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
                    List<SocketRole> roles = guildChannel.Guild.Roles.ToList();
                    Predicate<SocketRole> genderFinder = (SocketRole sr) => { return sr.Name == gender.ToString().ToLower(); };
                    SocketRole genderRole = roles.Find(genderFinder);
                    guildChannel.GetUser((ulong)userId).AddRoleAsync(genderRole);

                    // Remove old role
                    genderFinder = (SocketRole sr) => { return sr.Name == user.Gender.ToString().ToLower(); };
                    genderRole = roles.Find(genderFinder);
                    guildChannel.GetUser((ulong)userId).RemoveRoleAsync(genderRole);

                    user.Gender = gender;
                    db.SaveChanges();

                    message.Channel.SendMessageAsync($"Changed <@{userId}>'s gender to {gender.ToString().ToLower()}");

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
            User.Orientation orientation = Utilities.StringToOrientation(parameters[0]);

            if (orientation == User.Orientation.None)
            {
                message.Channel.SendMessageAsync("Couldn't understand that orientation... it can either be\n" +
                    "```\n" +
                    "- Straight\n" +
                    "- Gay\n" +
                    "- Bisexual\n" +
                    "- Asexual\n" +
                    "- Pansexual\n" +
                    "- Gray-a (if you'd rather it not be shown)\n" +
                    "```");

                return;
            }

            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Info(message.Author.Username, $"Changing @<{userId}> orientation to {orientation.ToString()}.");

                    SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
                    List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

                    // Remove old role
                    Predicate<SocketRole> orientationFinder = (SocketRole sr) => { return sr.Name == user.orientation.ToString().ToLower(); };
                    SocketRole orientationRole = roles.Find(orientationFinder);
                    guildChannel.GetUser(userId).RemoveRoleAsync(orientationRole);

                    // Adding role to user
                    orientationFinder = (SocketRole sr) => { return sr.Name == orientation.ToString().ToLower(); };
                    orientationRole = roles.Find(orientationFinder);
                    guildChannel.GetUser(userId).AddRoleAsync(orientationRole);


                    user.orientation = orientation;
                    db.SaveChanges();

                    message.Channel.SendMessageAsync($"<@{userId}> orientation is now {user.orientation.ToString().ToLower()}");

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
                ulong userId = message.Author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"user <@{userId}>'s fur set to: {parameters[0]}");

                    user.Fur = parameters[0];
                    db.SaveChanges();

                    message.Channel.SendMessageAsync($"<@{userId}> fur set to {parameters[0]}");  

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
                ulong userId = message.Author.Id;

                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    Logger.Warning(message.Author.Username, $"Setting <@{userId}>'s ref to {parameters[0]}");

                    user.Ref = parameters[0];
                    db.SaveChanges();

                    message.Channel.SendMessageAsync($"<@{userId}>'s ref has been set to {parameters[0]}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

}
