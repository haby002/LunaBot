using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.Database
{
    public static class UserExtensions
    {
        //public static void SetRef(this User user, string key, string value)
        //{
        //    Dictionary<string, string> extras = user.GetAllExtras();
        //    extras[key] = value;
        //    user.Ref = JsonConvert.SerializeObject(extras);
        //}

        //public static Dictionary<string,string> GetAllExtras(this User user)
        //{
        //    return JsonConvert.DeserializeObject<Dictionary<string, string>>(user.XmlExtra ?? "{}");
        //}

        //public static string GetExtra(this User user, string key)
        //{
        //    Dictionary<string, string> extras = user.GetAllExtras();

        //    if (extras.ContainsKey(key))
        //    {
        //        return extras[key];
        //    }
        //    else
        //    {
        //        return "";
        //    }
        //}
        
        /// <summary>
        /// Add XP to user and calculates if user has leveled up.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="words"></param>
        /// <returns>True if user leveled up, false otherwise.</returns>
        public static bool AddXP(this User user, int words)
        {
            user.Xp += words;

            if (user.Xp > (user.Level * 1500))
            {
                user.Level++;
                return true;
            }

            return false;
        }
    }
}
