using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.ServerUtilities
{
    class LobbyAnnouncements
    {
        private static IList<string> startupFlavorText = new List<string>()
        {
            "I'm up! I'm up... *yawn*",
            "```css\n#System (Ready)\n```",
            ":eyes:",
            "Totally did not just wake up...",
            "Someone called?",
            "And the moon rises!",
            "Thou has awoken from thou slumber!",
            "*Its ya ~~boi~~ girl*,",
            "I'm back bi***es!",
            "OwO"
        };


        public static async Task StartupConfirmationAsync(SocketTextChannel lobby)
        {
            Logger.Info("System", $"Announcing startup confirmation");
            
            Random r = new Random();
            
            await lobby.SendMessageAsync(startupFlavorText[r.Next(startupFlavorText.Count)]);
        }
    }
}
