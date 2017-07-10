using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot
{
    public static class Settings
    {
        private const string keyExtraAttributes = "EXTRA_ATTRIBUTES";

        public static T Get<T>(string key, T def = null) where T : class
        {
            using (DiscordContext db = new DiscordContext())
            {
                Setting setting = db.Settings.Where(x => x.SettingName == key).SingleOrDefault();

                if(setting == null)
                {
                    if (def == null)
                    {
                        throw new ArgumentException(string.Format("Setting {0} not found", key));
                    }
                    else
                    {
                        return def;
                    }
                }

                return Convert.ChangeType(setting.Value, typeof(T)) as T;
            }
        }

        public static void Set(string key, string value)
        {
            using (DiscordContext db = new DiscordContext())
            {
                Setting setting = db.Settings.Where(x => x.SettingName == key).SingleOrDefault();

                if (setting == null)
                {
                    setting = db.Settings.Create();
                    setting.SettingName = key;
                    setting.Value = value;
                    
                    db.Settings.Add(setting);
                    db.SaveChanges();
                }
                else
                {
                    setting.Value = value;
                    db.Settings.Attach(setting);
                    db.Entry(setting).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public static IList<string> GetExtraAttributes()
        {
            string attributeString = Get<string>(keyExtraAttributes, "");
            return new List<string>(attributeString.Split(';'));
        }
    }
}
