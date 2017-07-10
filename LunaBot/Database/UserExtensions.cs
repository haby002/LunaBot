﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.Database
{
    public static class UserExtensions
    {
        public static void SetExtra(this User user, string key, string value)
        {
            Dictionary<string, string> extras = user.GetAllExtras();
            extras[key] = value;
            user.XmlExtra = JsonConvert.SerializeObject(extras);
        }

        public static Dictionary<string,string> GetAllExtras(this User user)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(user.XmlExtra ?? "{}");
        }

        public static string GetExtra(this User user, string key)
        {
            Dictionary<string, string> extras = user.GetAllExtras();

            if (extras.ContainsKey(key))
            {
                return extras[key];
            }
            else
            {
                return "";
            }
        }
    }
}
