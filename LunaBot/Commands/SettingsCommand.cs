using Discord.WebSocket;
using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [LunaBotCommand("Settings")]
    class SettingsCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                if(parameters.Count() < 2)
                {
                    message.Channel.SendMessageAsync("Invalid settings command");
                    return;
                }

                string action = parameters[0].ToLower();
                string setting = parameters[1].ToLower();

                List<string> values = new List<string>(parameters);
                string value = string.Join(" ", values.GetRange(2, values.Count() - 2));

                if(action.Equals("get"))
                {
                    message.Channel.SendMessageAsync(Settings.Get<string>(setting));
                    return;
                }
                else if (action.Equals("set"))
                {
                    Settings.Set(setting, value);
                    message.Channel.SendMessageAsync("Setting value successfully updated");
                    return;
                }
            }
        }
    }
}
