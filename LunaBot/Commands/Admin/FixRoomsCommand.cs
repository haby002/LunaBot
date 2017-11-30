using System;
using System.Linq;
using Discord.WebSocket;
using LunaBot.Database;
using System.Collections.Generic;
using Discord.Rest;
using Discord.Rpc;

namespace LunaBot.Commands
{
    [LunaBotCommand("FixRooms")]
    class FixRoomsCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                ulong userId = message.Author.Id;
                
                Logger.Verbose(message.Author.Username, "Fixing rooms...");
                RestUserMessage myMessage = message.Channel.SendMessageAsync("Fixing rooms...").Result;

                var channel = message.Channel as SocketGuildChannel;
                var guildChannels = channel.Guild.TextChannels;
                foreach(SocketTextChannel ch in guildChannels)
                {
                    if (/*ch.Name.Contains("room-") || */ch.Name.Contains("void-") || ch.Name.Contains("intro-"))
                    {
                        Logger.Verbose("system", $"Found: {ch.Name}");
                        ch.DeleteAsync();
                    }
                    else
                    {
                        switch (ch.Name)
                        {
                            case "rules_and_announcements":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "welcome":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "lobby":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "games-lobby":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "suggestion-box":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "music":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "atelier":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "playroom":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "pool":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "staff":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "bot_room":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "meme-dungeon":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            case "nsfw-nightclub":
                                Logger.Verbose("system", $"Found room {ch.Name}");
                                break;
                            default:
                                Logger.Warning("system", $"Found room {ch.Name}");
                                // ch.DeleteAsync();
                                break;
                        }
                    }
                }

                /*foreach(SocketGuildUser rgu in channel.Guild.Users)
                {
                    RestTextChannel rtc = await channel.Guild.CreateTextChannelAsync($"room-{rgu.Id}");
                    var addChannel = channel.Guild.GetChannel(rtc.Id);
                    
                    Discord.OverwritePermissions op = addChannel.GetPermissionOverwrite(rgu).Value;
                    op.ToAllowList();
                    op.Modify(readMessages: 0);
                    await addChannel.AddPermissionOverwriteAsync(rgu, op);

                    op = addChannel.GetPermissionOverwrite(channel.Guild.EveryoneRole).Value;
                    op.ToDenyList();
                    op.Modify(readMessages: (Discord.PermValue)1);
                    await addChannel.AddPermissionOverwriteAsync(channel.Guild.EveryoneRole, op);

                }*/
                Logger.Verbose(message.Author.Username, $"{guildChannels.Count} Rooms Fixed!");
                message.Channel.SendMessageAsync("Rooms Fixed!");
            }
        }
    }
}
