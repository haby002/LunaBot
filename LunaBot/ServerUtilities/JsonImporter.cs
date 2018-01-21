using Discord.WebSocket;
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

            
        }
    }
}
