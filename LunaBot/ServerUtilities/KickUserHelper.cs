using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunaBot.ServerUtilities
{
    class KickUserHelper
    {
        private static IList<string> kickFlavorText = new List<string>()
        {
            "{0} bit the dust",
            "Critical hit! {0} has been kicked!",
            "I cast purge on {0}! Begone demon!",
            "The last of the {0} has been ~~slain~~ kicked.",
            "No more of thee {0}!",
            "Find some other server to bother {0}",
            "Boot to {0}'s face!",
            "Bye bye {0}"
        };


        public static async System.Threading.Tasks.Task KickAsync(SocketTextChannel channel, SocketGuildUser user)
        {
            if (user.Id == 333285108402487297)
                return;

            Logger.Info("System", $"Kicking {user.Username}");

            try
            {
                await user.SendMessageAsync("You have been kicked from the server from inactivity.\n" +
                    "You can join again but once you get kicked 3 times will be banned.\n" +
                    "*Hint: Prevent getting kicked by being part of the community.*\n" +
                    "https://discord.gg/J4c8wKg");
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (HttpException e)
            {
                Logger.Info("System", $"{user.Username} blocks DMs.");
            }
#pragma warning restore CS0168 // Variable is declared but never used

            Random r = new Random();
            
            await user.KickAsync("Purged for inactivity");
            await channel.SendMessageAsync(String.Format(kickFlavorText[r.Next(kickFlavorText.Count)], user.Username));
        }
    }
}
