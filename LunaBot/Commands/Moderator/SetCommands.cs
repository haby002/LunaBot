using System;
using System.Linq;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("set")]
    class SetCommands : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            // Check if command params are correct.
            if(parameters.Length < 3)
            {
                Logger.Verbose(message.Author.Username, "Failed database modify command");
                message.Channel.SendMessageAsync("Error: Wrong syntax, try !set `user` `property` `value`.");

                return;
            }

            string[] unparsedUserId = parameters[0].Split(new [] {'@', '>'});
            
            // Check if user attached is correct.
            if(unparsedUserId.Length < 2|| unparsedUserId[0] != "<" || unparsedUserId[1].Length != 18)
            {
                Logger.Verbose(message.Author.Username, "Failed database modify command");
                message.Channel.SendMessageAsync("Error: Command requires an attached `user` to command. Forgot the '@'?");

                return;
            }
            
            using (DiscordContext db = new DiscordContext())
            {
                long userId = Convert.ToInt64(message.Author.Id);
                User user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
                if (user != null)
                {
                    switch (parameters[1].ToLower())
                    {
                        case "description":
                        case "desc":
                            user.Description = String.Join(" ", parameters.Skip(2).Where(s => !String.IsNullOrEmpty(s)));
                            message.Channel.SendMessageAsync($"Success: {parameters[0]}'s description updated");
                            break;
                        case "level":
                            if(int.TryParse(parameters[2], out int n))
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s level from {user.Level} to {parameters[2]}");
                                user.Level = Convert.ToInt32(parameters[2]);
                                message.Channel.SendMessageAsync($"Success: {parameters[0]}'s level set to `{user.Level}`");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set level command");
                                message.Channel.SendMessageAsync($"Error: Level requires a number to set. You gave: `{parameters[2]}`");
                            }
                            break;
                        case "xp":
                            if (int.TryParse(parameters[2], out int o))
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s xp from {user.Xp} to {parameters[2]}");
                                user.Xp = Convert.ToInt32(parameters[2]);
                                message.Channel.SendMessageAsync($"Success: {parameters[0]}'s xp set to `{user.Xp}`");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set xp command");
                                message.Channel.SendMessageAsync($"Error: XP requires a number to set. You gave: `{parameters[2]}`");
                            }
                            break;
                        case "age":
                            if (int.TryParse(parameters[2], out int m))
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s age from {user.Age} to {parameters[2]}");
                                user.Age = Convert.ToInt32(parameters[2]);
                                message.Channel.SendMessageAsync($"Success: {parameters[0]}'s age set to `{user.Age}`");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set age command");
                                message.Channel.SendMessageAsync($"Error: Age requires a number to set. You gave: `{parameters[2]}`");
                            }
                            break;
                        case "gender":
                            if(parameters[2].ToLower() == "male" || parameters[2].ToLower() == "female" || parameters[2].ToLower() == "other")
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s gender from {user.Gender} to {parameters[2]}");
                                user.Gender = parameters[2];
                                message.Channel.SendMessageAsync($"Success: {parameters[0]}'s gender set to `{user.Gender}`");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set gender command");
                                message.Channel.SendMessageAsync($"Error: gender can only be `male`, `female`, or `other`. You gave: {parameters[2]}");
                            }
                            break;
                        case "ref":
                            if (Uri.TryCreate(parameters[2], UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                            {
                                Logger.Info(message.Author.Username, $"Changed user {parameters[0]}'s ref from {user.Ref} to {parameters[2]}");
                                user.Ref = parameters[2];
                                message.Channel.SendMessageAsync($"Success: {parameters[0]}'s ref has been updated");
                            }
                            else
                            {
                                Logger.Warning(message.Author.Username, "Failed database set ref command");
                                message.Channel.SendMessageAsync($"Error: Ref sheet must be a link. You gave: `{parameters[2]}`");
                            }
                            break;
                        default:
                            Logger.Warning(message.Author.Username, "Failed database set command.");
                            message.Channel.SendMessageAsync($"Error: Could not find attribute {parameters[1]}. Check you syntax!");
                            return;
                    }

                    db.SaveChanges();
                    Logger.Verbose(message.Author.Username, $"Updated data for {userId}");

                    return;
                }

                Logger.Verbose(message.Author.Username, $"Failed to find user: {userId}");
                message.Channel.SendMessageAsync($"Failed to find user: {userId}");

                //Logger.Verbose(message.Author.Username, "Created User");
                //message.Channel.SendMessageAsync("Created User");

                Logger.Verbose("System", $"Updated information for user {message.Author}");
            }
        }
    }
}
