using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using LunaBot.Database;

namespace LunaBot.Commands
{
    [LunaBotCommand("Set")]
    class SetCommand : BaseCommand
    {
        public override void Process(SocketMessage message, string[] parameters)
        {
            using (DiscordContext db = new DiscordContext())
            {
                string field = parameters[0].ToLower();
                List<string> values = new List<string>(parameters);
                string value = string.Join(" ", values.GetRange(1, values.Count() - 1));
                long userId = Convert.ToInt64(message.Author.Id);
                User user = db.Users.Where(x => x.DiscordId == userId).SingleOrDefault();

                if(user == null)
                {
                    message.Channel.SendMessageAsync("You need to register first with !register");
                    return;
                }

                if(field.Equals("desc") || field.Equals("description"))
                {
                    user.Description = value;
                }
                else if(field.Equals("age"))
                {
                    int age = 0;
                    try
                    {
                        age = Convert.ToInt32(value);
                    }
                    catch(FormatException)
                    {
                        message.Channel.SendMessageAsync("Age must be a number");
                        return;
                    }
                    user.Age = age;
                }
                else if(field.Equals("gender") || field.Equals("sex"))
                {
                    try
                    {
                        User.Genders? gender = Enum.Parse(typeof(User.Genders), value, true) as User.Genders?;
                        if (gender == null)
                        {
                            message.Channel.SendMessageAsync("Unrecognized gender, must be Male or Female");
                            return;
                        }
                        user.Gender = gender.Value;
                    }
                    catch(ArgumentException)
                    {
                        message.Channel.SendMessageAsync("Unrecognized gender, must be Male or Female");
                    }
                }
                else
                {
                    if (Settings.GetExtraAttributes().Contains(field))
                    {
                        user.SetExtra(field, value);
                    }
                    else
                    {
                        message.Channel.SendMessageAsync("Unrecognized attribute");
                        return;
                    }
                }

                db.Users.Attach(user);
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
