using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunaBot.ServerUtilities
{
    class MuteUserHelper
    {
        private static IList<string> kickFlavorText = new List<string>()
        {
            "You've said enough <@{0}>",
            "Your colloquialism isn't welcomed for now <@{0}>",
            "BAM! <@{0}> has been muted!",
            "Don't want to lose that mouth, do you <@{0}>?",
            "Chillax my dude. <@{0}> muted.",
            "A wise man once said... hush <@{0}>"
        };


        public static async void mute(SocketTextChannel channel, SocketGuildUser user, int seconds)
        {
            if (user.Id == 333285108402487297)
                return;

            // Logging, telling the user, and announcing in server.
            Logger.Info("System", $"Muting {user.Username}");
            await user.SendMessageAsync($"You have been muted for {seconds} seconds");
            Random r = new Random();
            await channel.SendMessageAsync(String.Format(kickFlavorText[r.Next(kickFlavorText.Count)], user.Id));

            Predicate<SocketRole> muteFinder;
            SocketRole mute;
            List<SocketRole> roles = channel.Guild.Roles.ToList();

            // Set mute role
            muteFinder = (SocketRole sr) => { return sr.Name == Roles.Mute; };
            mute = roles.Find(muteFinder);
            await user.AddRoleAsync(mute);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                Thread.Sleep(seconds * 1000);
                await user.RemoveRoleAsync(mute);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
