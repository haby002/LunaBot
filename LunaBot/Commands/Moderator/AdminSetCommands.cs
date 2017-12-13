using System;
using System.Linq;
using Discord.WebSocket;
using LunaBot.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("set")]
    class AdminSetCommands : BaseCommand
    {
        public override async Task Process(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if(parameters.Length < 3)
            {
                Logger.Verbose(message.Author.Username, "Failed database modify command");
                await message.Channel.SendMessageAsync("Error: Wrong syntax, try !set `user` `property` `value`.");

                return;
            }
            
            
            // Check if user attached is correct.
            if(message.MentionedUsers.Count == 0)
            {
                Logger.Verbose(message.Author.Username, "Failed database modify command");
                await message.Channel.SendMessageAsync("Error: Command requires an attached `user` to the command. Forgot the '@'?");

                return;
            }
            
            using (DiscordContext db = new DiscordContext())
            {
                // check privileges
                ulong userId = message.Author.Id;
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if((int)user.Privilege < 1)
                {
                    Logger.Warning(message.Author.Username, "Not enough permissions.");
                    await message.Channel.SendMessageAsync("Can't let you do that Dave.");
                    return;
                }

                // Modify given user
                userId = message.MentionedUsers.FirstOrDefault().Id;
                user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    switch (parameters[1].ToLower())
                    {
                        case "description":
                        case "desc":
                            Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s description from {user.Description} to {parameters[2]}");
                            user.Description = parameters[2];
                            await message.Channel.SendMessageAsync($"Success: {parameters[0]}'s description updated to {parameters[2]}");
                            break;
                        case "fur":
                        case "f":
                            Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s fur from {user.Fur} to {parameters[2]}");
                            user.Fur = parameters[2];
                            await message.Channel.SendMessageAsync($"Success: {parameters[0]}'s fur updated to {parameters[2]}");
                            break;
                        case "level":
                        case "lvl":
                            if(int.TryParse(parameters[2], out int n))
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s level from {user.Level} to {parameters[2]}");
                                user.Level = Convert.ToInt32(parameters[2]);
                                user.Xp = 0;
                                await message.Channel.SendMessageAsync($"Success: {parameters[0]}'s level set to `{user.Level}`");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set level command");
                                await message.Channel.SendMessageAsync($"Error: Level requires a number to set. You gave: `{parameters[2]}`");
                            }
                            break;
                        case "xp":
                            if (int.TryParse(parameters[2], out int o))
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s xp from {user.Xp} to {parameters[2]}");
                                user.Xp = Convert.ToInt32(parameters[2]);
                                await message.Channel.SendMessageAsync($"Success: {parameters[0]}'s xp set to `{user.Xp}`");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set xp command");
                                await message.Channel.SendMessageAsync($"Error: XP requires a number to set. You gave: `{parameters[2]}`");
                            }
                            break;
                        case "age":
                        case "a":
                            if (int.TryParse(parameters[2], out int m))
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s age from {user.Age} to {parameters[2]}");
                                user.Age = Convert.ToInt32(parameters[2]);
                                await message.Channel.SendMessageAsync($"Success: {parameters[0]}'s age set to `{user.Age}`");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set age command");
                                await message.Channel.SendMessageAsync($"Error: Age requires a number to set. You gave: `{parameters[2]}`");
                            }
                            break;
                        case "gender":
                        case "g":
                            Predicate<SocketRole> genderFinder;
                            SocketRole gender;
                            SocketTextChannel channel = message.Channel as SocketTextChannel;
                            List<SocketRole> roles = channel.Guild.Roles.ToList();
                            SocketGuildUser discordUser = message.MentionedUsers.FirstOrDefault() as SocketGuildUser;

                            // Remove old role
                            genderFinder = (SocketRole sr) => { return sr.Name == user.Gender.ToString().ToLower(); };
                            gender = roles.Find(genderFinder);
                            await discordUser.RemoveRoleAsync(gender);

                            // Add new role
                            switch (parameters[2].ToLower())
                            {
                                case "male":
                                case "m":
                                    genderFinder = (SocketRole sr) => { return sr.Name == "male"; };
                                    gender = roles.Find(genderFinder);
                                    await discordUser.AddRoleAsync(gender);
                                    user.Gender = User.Genders.Male;
                                    break;
                                case "female":
                                case "f":
                                    genderFinder = (SocketRole sr) => { return sr.Name == "female"; };
                                    gender = roles.Find(genderFinder);
                                    await discordUser.AddRoleAsync(gender);
                                    user.Gender = User.Genders.Female;
                                    break;
                                case "other":
                                case "o":
                                    genderFinder = (SocketRole sr) => { return sr.Name == "other"; };
                                    gender = roles.Find(genderFinder);
                                    await discordUser.AddRoleAsync(gender);
                                    user.Gender = User.Genders.Other;
                                    break;
                                case "trans-male":
                                    genderFinder = (SocketRole sr) => { return sr.Name == "trans-male"; };
                                    gender = roles.Find(genderFinder);
                                    await discordUser.AddRoleAsync(gender);
                                    user.Gender = User.Genders.TransM;
                                    break;
                                case "trans-female":
                                    genderFinder = (SocketRole sr) => { return sr.Name == "trans-female"; };
                                    gender = roles.Find(genderFinder);
                                    await discordUser.AddRoleAsync(gender);
                                    user.Gender = User.Genders.TransF;
                                    break;
                                default:
                                    await message.Channel.SendMessageAsync("I'm sorry I couldn't understand your message. Make sure the gender is either male, female, trans-male, trans-female, or other.\n" +
                                        $"You gave: {parameters[2]}");
                                    return;
                            }
                            Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s gender from {user.Gender} to {parameters[2]}");
                            await message.Channel.SendMessageAsync($"Success: {parameters[0]}'s gender set to `{user.Gender}`");
                            break;
                        case "orientation":
                        case "o":
                            Predicate<SocketRole> orientationFinder;
                            SocketRole orientation;
                            channel = message.Channel as SocketTextChannel;
                            roles = channel.Guild.Roles.ToList();
                            discordUser = message.MentionedUsers.FirstOrDefault() as SocketGuildUser;

                            // Remove old role
                            orientationFinder = (SocketRole sr) => { return sr.Name == user.orientation.ToString().ToLower(); };
                            orientation = roles.Find(orientationFinder);
                            await discordUser.RemoveRoleAsync(orientation);

                            // Add new role
                            switch (parameters[2])
                            {
                                case "straight":
                                case "s":
                                    orientationFinder = (SocketRole sr) => { return sr.Name == "straight"; };
                                    orientation = roles.Find(orientationFinder);
                                    await discordUser.AddRoleAsync(orientation);
                                    user.orientation = User.Orientation.Straight;
                                    break;
                                case "gay":
                                case "g":
                                    orientationFinder = (SocketRole sr) => { return sr.Name == "gay"; };
                                    orientation = roles.Find(orientationFinder);
                                    await discordUser.AddRoleAsync(orientation);
                                    user.orientation = User.Orientation.Gay;
                                    break;
                                case "bisexual":
                                case "bi":
                                    orientationFinder = (SocketRole sr) => { return sr.Name == "bisexual"; };
                                    orientation = roles.Find(orientationFinder);
                                    await discordUser.AddRoleAsync(orientation);
                                    user.orientation = User.Orientation.Bi;
                                    break;
                                case "asexual":
                                    orientationFinder = (SocketRole sr) => { return sr.Name == "asexual"; };
                                    orientation = roles.Find(orientationFinder);
                                    await discordUser.AddRoleAsync(orientation);
                                    user.orientation = User.Orientation.Asexual;
                                    break;
                                case "gray-a":
                                    orientationFinder = (SocketRole sr) => { return sr.Name == "gray-a"; };
                                    orientation = roles.Find(orientationFinder);
                                    await discordUser.AddRoleAsync(orientation);
                                    user.orientation = User.Orientation.Gray;
                                    break;
                                case "pansexual":
                                case "pan":
                                    orientationFinder = (SocketRole sr) => { return sr.Name == "pan"; };
                                    orientation = roles.Find(orientationFinder);
                                    await discordUser.AddRoleAsync(orientation);
                                    user.orientation = User.Orientation.Pansexual;
                                    break;
                                default:
                                    await message.Channel.SendMessageAsync("Hmm... That's not an orientation I can undestand.\n" +
                                        "Make sure it's either straight, gay, bisexaul, asexual, pansexual, or gray-a.");
                                    return;
                            }
                            Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s orientation to {user.orientation.ToString()}");
                            await message.Channel.SendMessageAsync($"Success: {parameters[0]}'s orientation set to `{user.orientation.ToString()}`");
                            break;
                        case "ref":
                            if (Uri.TryCreate(parameters[2], UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s ref from {user.Ref} to {parameters[2]}");
                                user.Ref = parameters[2];
                                await message.Channel.SendMessageAsync($"Success: {parameters[0]}'s ref has been updated");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set ref command");
                                await message.Channel.SendMessageAsync($"Error: Ref sheet must be a link. You gave: `{parameters[2]}`");
                            }
                            break;
                        default:
                            Logger.Warning(message.Author.Username, "Failed database set command.");
                            await message.Channel.SendMessageAsync($"Error: Could not find attribute {parameters[1]}. Check you syntax!");
                            return;
                    }
                    
                    db.SaveChanges();
                    Logger.Verbose(message.Author.Username, $"Updated data for {userId}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                await message.Channel.SendMessageAsync($"Failed to find user: {userId}");

                //Logger.Verbose(message.Author.Username, "Created User");
                //message.Channel.SendMessageAsync("Created User");

                Logger.Verbose("System", $"Updated information for user {message.Author}");
            }
        }
    }
}
