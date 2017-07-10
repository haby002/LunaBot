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
                if(parameters.Count() < 1)
                {
                    message.Channel.SendMessageAsync("Invalid settings command");
                    return;
                }

                string action = parameters[0].ToLower();
                string setting = "";
                string value = "";

                if(parameters.Count() > 1)
                {
                    setting = parameters[1].ToLower();
                }

                if(parameters.Count() > 2)
                {
                    List<string> values = new List<string>(parameters);
                    value = string.Join(" ", values.GetRange(2, values.Count() - 2));
                }

                if(action.Equals("get"))
                {
                    if(setting.Equals(string.Empty))
                    {
                        message.Channel.SendMessageAsync(this.DisplayAllSettings());
                    }
                    else
                    {
                        message.Channel.SendMessageAsync(Settings.Get<string>(setting));
                        return;
                    }
                }
                else if (action.Equals("set"))
                {
                    Settings.Set(setting, value);
                    message.Channel.SendMessageAsync("Setting value successfully updated");
                    return;
                }
            }
        }

        private string DisplayAllSettings()
        {
            IDictionary<string, string> settings = Settings.GetAll();
            string toDisplay = "";
            foreach (KeyValuePair<string, string> entry in settings)
            {
                toDisplay += $"\n{entry.Key}: {entry.Value}";
            }

            return toDisplay;
        }
    }
}
