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
            "Spinning engines...",
            "Another day, another chip",
            "Luna est super vos..",
            "Someone dinged my bell?",
            "Upgrade complete!",
            "Are we open yet?",
            "Don't forget to try our amazing Porcuwine!",
            "*beep boop*",
            "Is it Bot-mas already?"
        };


        public static async Task StartupConfirmationAsync(SocketTextChannel lobby)
        {
            Logger.Info("System", $"Announcing startup confirmation");
            
            Random r = new Random();
            
            await lobby.SendMessageAsync(startupFlavorText[r.Next(startupFlavorText.Count)]);
        }
    }
}
