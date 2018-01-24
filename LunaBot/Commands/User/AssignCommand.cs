using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace LunaBot.Commands
{
    [LunaBotCommand("Assign")]
    class AscendCommand : BaseCommand
    {
        public override async Task ProcessAsync(SocketMessage message, string[] parameters)
        {
            ulong user = message.Author.Id;

            string roleName = parameters[0];

            SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;
            List<SocketRole> roles = guildChannel.Guild.Roles.ToList();


            Predicate<SocketRole> roleFinder = (SocketRole sr) => { return sr.Name == roleName; };
            SocketRole role = roles.Find(roleFinder);
            if (role != null)
            {
                await guildChannel.GetUser((ulong)user).AddRoleAsync(role);

                await message.Channel.SendMessageAsync($"role given");

            }
            else
            {
                await message.Channel.SendMessageAsync($"ERROR");
            }
        }
    }
}