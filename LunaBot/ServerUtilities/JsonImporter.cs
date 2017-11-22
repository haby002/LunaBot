using Discord.WebSocket;
using LunaBot.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaBot.ServerUtilities
{
    public class JsonImporter
    {
        /// <summary>
        /// Serializes json dumps from redis caches
        /// </summary>
        /// <param name="guild"></param>
        public static void json(SocketGuild guild)
        {
            string json = System.IO.File.ReadAllText(@"C:\Users\Javier\Downloads\data.json");

            Dictionary<string, object> toplevel = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            List<SocketGuildUser> users = guild.Users.ToList();

            using (DiscordContext db = new DiscordContext())
            {
                foreach (SocketGuildUser u in users)
                {
                    if (toplevel.ContainsKey(u.Id.ToString()))
                    {
                        User user = db.Users.Where(x => x.DiscordId == (long)u.Id).FirstOrDefault();

                        JObject secondLevel = (JObject)toplevel[u.Id.ToString()];

                        JToken thirdLevel = secondLevel.Last;

                        JToken fourthLevel = thirdLevel.First();

                        foreach (JProperty attribute in fourthLevel)
                        {
                            string value;
                            switch (attribute.Name)
                            {
                                case "age":
                                    user.Age = int.Parse(attribute.Value.ToString());
                                    break;
                                case "sex":
                                    value = attribute.Value.ToString();
                                    break;
                                case "level":
                                    user.Level = int.Parse(attribute.Value.ToString());
                                    break;
                                case "fur":
                                    user.Fur = attribute.Value.ToString();
                                    break;
                                case "ref":
                                    user.Ref = attribute.Value.ToString();
                                    break;
                                case "desc":
                                    user.Description = attribute.Value.ToString();
                                    break;
                            }
                        }
                    }
                }

                db.SaveChanges();
            }
        }
    }
}
