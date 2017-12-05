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
        /// <summary>
        /// Add XP to user and calculates if user has leveled up.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="words"></param>
        /// <returns>True if user leveled up, false otherwise.</returns>
        public static bool AddXP(this User user, int words)
        {
            user.Xp += words;

            if (user.Xp > (user.Level * 150))
            {
                user.Xp = user.Xp - (user.Level * 150);
                user.Level++;
                return true;
            }

            return false;
        }

        public static bool ResetUser(this User user)
        {
            try
            {
                user.Nickname = null;
                user.Age = 0;
                user.Description = null;
                user.Fur = null;
                user.Gender = User.Genders.None;
                user.Monk = false;
                user.Nsfw = false;
                user.orientation = User.Orientation.None;
                user.Ref = null;
                user.TutorialFinished = false;
            }
            catch (Exception e)
            {
                e.Log();

                return false;
            }

            return true;
        }
    }
}
