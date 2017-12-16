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
            "lol gay, kicked {0}",
            "Critical hit! {0} has been kicked!",
            "I cast purge on {0}! Begone demon!",
            "2 to the 1 to the 3, {0} has been kicked and you can't disagree!",
            "The last of the {0} has been ~~slain~~ kicked."
        };


        public static async void kick(SocketTextChannel channel, SocketGuildUser user)
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
            catch(HttpException e)
            {
                Logger.Info("System", $"{user.Username} blocks DMs.");
            }
            Random r = new Random();
            
            await user.KickAsync("Purged for inactivity");
            await channel.SendMessageAsync(String.Format(kickFlavorText[r.Next(kickFlavorText.Count)], user.Username));
        }
    }
}
