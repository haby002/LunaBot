using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("set_Desc", "set_Description")]
    class SetDescCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
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

                    await message.Channel.SendMessageAsync($"Changed <@{userId}>'s description to: {user.Description}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_Age", "set_a")]
    class SetAgeCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            if(!int.TryParse(parameters[0], out int age))
            {
                await message.Channel.SendMessageAsync("The numbers Mason, what are the numbers?!");

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

                    await message.Channel.SendMessageAsync($"Changed <@{userId}> age to `{user.Age}` years old.");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_g", "set_gender")]
    class SetGenderCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            User.Genders gender = Utilities.StringToGender(parameters[0]);
            if (gender == User.Genders.None)
            {
                await message.Channel.SendMessageAsync("Couldn't understand that gender... it can either be\n" +
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
                    Logger.Warning("AdminSetCmd", $"Setting @<{userId}>'s gender to {gender.ToString()}.");
                    
                    SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
                    List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

                    // Remove old role
                    Predicate<SocketRole> genderFinder = (SocketRole sr) => { return sr.Name == user.Gender.ToString().ToLower(); };
                    SocketRole genderRole = roles.Find(genderFinder);
                    genderRole = roles.Find(genderFinder);
                    if (genderRole != null)
                    {
                        await guildChannel.GetUser((ulong)userId).RemoveRoleAsync(genderRole);
                        Logger.Verbose("System", $"found role {genderRole.Name} and removed it.");
                    }
                    else
                    {
                        Logger.Warning("System", $"Couldn't find role {user.Gender.ToString().ToLower()}.");
                    }

                    // Add new role
                    genderFinder = (SocketRole sr) => { return sr.Name == gender.ToString().ToLower(); };
                    genderRole = roles.Find(genderFinder);
                    await guildChannel.GetUser((ulong)userId).AddRoleAsync(genderRole);

                    user.Gender = gender;
                    db.SaveChanges();

                    await message.Channel.SendMessageAsync($"Changed <@{userId}>'s gender to {gender.ToString().ToLower()}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_o", "set_Orientation")]
    class SetOrientationCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            User.Orientation orientation = Utilities.StringToOrientation(parameters[0]);

            if (orientation == User.Orientation.None)
            {
                await message.Channel.SendMessageAsync("Couldn't understand that orientation... it can either be\n" +
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
                    if (orientationRole != null)
                    {
                        await guildChannel.GetUser(userId).RemoveRoleAsync(orientationRole);
                        Logger.Verbose("System", $"found role {orientationRole.Name} and removed it.");
                    }
                    else
                    {
                        Logger.Warning("System", $"Couldn't find role {user.orientation.ToString().ToLower()}.");
                    }

                    // Adding role to user
                    orientationFinder = (SocketRole sr) => { return sr.Name == orientation.ToString().ToLower(); };
                    orientationRole = roles.Find(orientationFinder);
                    await guildChannel.GetUser(userId).AddRoleAsync(orientationRole);


                    user.orientation = orientation;
                    db.SaveChanges();

                    await message.Channel.SendMessageAsync($"<@{userId}> orientation is now {user.orientation.ToString().ToLower()}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_Fur", "set_f")]
    class SetFurCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
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

                    await message.Channel.SendMessageAsync($"<@{userId}> fur set to {parameters[0]}");  

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_Ref", "set_f")]
    class SetRefCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
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

                    await message.Channel.SendMessageAsync($"<@{userId}>'s ref has been set to {parameters[0]}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

            }
        }
    }

    [LunaBotCommand("set_Sfw")]
    class SetSFWCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            string sfwAsk = parameters[0].ToLower();
            if (sfwAsk == "yes" || sfwAsk == "no")
            {
                using (DiscordContext db = new DiscordContext())
                {
                    ulong userId = message.Author.Id;

                    User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                    if (user != null)
                    {
                        SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
                        List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

                        if (sfwAsk == "no")
                        {
                            Logger.Info(message.Author.Username, $"Removeing SFW role for @<{userId}>.");

                            // Remove old role
                            Predicate<SocketRole> SfwFinder = (SocketRole sr) => { return sr.Name == "SFW"; };
                            SocketRole SfwRole = roles.Find(SfwFinder);
                            if (SfwRole != null)
                            {
                                await guildChannel.GetUser(userId).RemoveRoleAsync(SfwRole);
                                Logger.Verbose("System", $"found role {SfwRole.Name} and removed it.");
                                await message.Channel.SendMessageAsync($"<@{userId}> is now alowed into the NSFW rooms.");
                            }
                            else
                            {
                                Logger.Warning("System", $"Couldn't find role SWF.");
                            }
                        }
                        else if (sfwAsk == "yes")
                        {
                            // Adding role to user
                            Predicate<SocketRole> Sfwfinder = (SocketRole sr) => { return sr.Name == "SFW"; };
                            SocketRole SfwRole = roles.Find(Sfwfinder);
                            await guildChannel.GetUser(userId).AddRoleAsync(SfwRole);
                            if (SfwRole != null)
                            {
                                await message.Channel.SendMessageAsync($"<@{userId}> has been removed from the NSFW rooms.");
                            }
                            else
                            {
                                Logger.Warning("System", $"Couldn't find role SWF.");
                            }
                        }

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                    await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

                }
            }
            else
            {
                await message.Channel.SendMessageAsync($"Sorry i couldn't understand. Please answer `yes` or `no`");

            }
        }
    }

    [LunaBotCommand("set_Monk")]
    class SetMonkCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            string monkAsk = parameters[0].ToLower();
            if (monkAsk == "yes" || monkAsk == "no")
            {
                using (DiscordContext db = new DiscordContext())
                {
                    ulong userId = message.Author.Id;

                    User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                    if (user != null)
                    {
                        SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
                        List<SocketRole> roles = guildChannel.Guild.Roles.ToList();

                        if (monkAsk == "no")
                        {
                            Logger.Info(message.Author.Username, $"Removing Monk role for @<{userId}>.");

                            // Remove old role
                            Predicate<SocketRole> MonkFinder = (SocketRole sr) => { return sr.Name == "Monk"; };
                            SocketRole MonkRole = roles.Find(MonkFinder);
                            if (MonkRole != null)
                            {
                                await guildChannel.GetUser(userId).RemoveRoleAsync(MonkRole);
                                Logger.Verbose("System", $"found role {MonkRole.Name} and removed it.");
                                await message.Channel.SendMessageAsync($"<@{userId}> is now alowed into the RP rooms.");
                            }
                            else
                            {
                                Logger.Warning("System", $"Couldn't find role Monk.");
                            }
                        }
                        else if (monkAsk == "yes")
                        {


                            // Adding role to user
                            Predicate<SocketRole> Monkfinder = (SocketRole sr) => { return sr.Name == "Monk"; };
                            SocketRole MonkRole = roles.Find(Monkfinder);
                            await guildChannel.GetUser(userId).AddRoleAsync(MonkRole);
                            if (MonkRole != null)
                            {
                                await message.Channel.SendMessageAsync($"<@{userId}> has been removed from RP rooms.");
                            }
                            else
                            {
                                Logger.Warning("System", $"Couldn't find role Monk.");
                            }
                        }

                        return;
                    }

                    Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                    await message.Channel.SendMessageAsync($"Failed to find user: `{message.Author}`");

                }
            }
            else
            {
                await message.Channel.SendMessageAsync($"Sorry i couldn't understand. Please answer `yes` or `no`");

            }
        }
    }
}
