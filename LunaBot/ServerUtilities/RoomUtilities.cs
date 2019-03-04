using Discord.Rest;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace LunaBot.ServerUtilities
{
    class RoomUtilities
    {
        public static async Task<RestTextChannel> CreatePersonalRoomAsync(SocketGuild guild, SocketGuildUser user)
        {
            SocketRole everyone = guild.Roles.Where(x => x.IsEveryone).First();

            // Creat personal room
            RestTextChannel personalRoom = await guild.CreateTextChannelAsync($"room-{user.Id}");

            // Make room only visible to new user and bots
            await personalRoom.AddPermissionOverwriteAsync(user, Permissions.roomPerm);
            await personalRoom.AddPermissionOverwriteAsync(everyone, Permissions.removeAllPerm);

            // Send intro information
            await personalRoom.SendMessageAsync($"<@{user.Id}>, welcome to your room! \n" +
                $"The server might be public but this is your own private sliver of the server.\n" +
                $"You can run commands, save images, post stuff, etc.\n" +
                $"type `!help` for a list of the commands!");

            return personalRoom;
        }
    }
}
