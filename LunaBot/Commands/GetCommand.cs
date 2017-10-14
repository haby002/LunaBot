using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("Get")]
    class GetCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                string field = parameters[0].ToLower();
                long userId = Convert.ToInt64(message.Author.Id);
                bool isSelf = true;
                if (parameters.Count() != 1)
                {
                    userId = Convert.ToInt64(parameters[1]);
                    isSelf = false;
                }

                User user = db.Users.Where(x => x.DiscordId == userId).SingleOrDefault();

                if (user == null)
                {
                    if(isSelf)
                    {
                        message.Channel.SendMessageAsync("You need to register first with !register");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync("User is not yet registered");
                    }
                    
                    return;
                }

                string toDisplay = "";
                if (field.Equals("all"))
                {
                    toDisplay = $@" Displaying all set attributes:
Description: {user.Description}
Age: {user.Age}
Gender: {user.Gender}";
                    //foreach(KeyValuePair<string,string> entry in user.GetAllExtras())
                    //{
                    //    toDisplay += $"\n{entry.Key}: {entry.Value}";
                    //}
                }
                else if (field.Equals("desc") || field.Equals("description"))
                {
                    toDisplay = user.Description;
                }
                else if (field.Equals("age"))
                {
                    toDisplay = user.Age.ToString();
                }
                else if (field.Equals("gender") || field.Equals("sex"))
                {
                    toDisplay = user.Gender.ToString();
                }
                else
                {
                    //if (Settings.GetExtraAttributes().Contains(field))
                    //{
                    //    toDisplay = user.GetExtra(field);
                    //}
                    //else
                    //{
                    //    message.Channel.SendMessageAsync("Unrecognized attribute");
                    //    return;
                    //}
                }

                message.Channel.SendMessageAsync(string.Format("{0}: {1}", field, toDisplay));
            }
        }
    }
}
