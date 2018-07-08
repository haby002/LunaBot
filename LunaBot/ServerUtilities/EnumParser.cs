using LunaBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.ServerUtilities
{
    class EnumParsers
    {
        public static User.Genders StringToGender(string gender)
        {
            switch (gender.ToLower())
            {
                case "male":
                case "m":
                    return User.Genders.Male;
                case "female":
                case "fe":
                    return User.Genders.Female;
                case "trans-male":
                case "trans-m":
                    return User.Genders.Trans_Male;
                case "trans-female":
                case "trans-f":
                    return User.Genders.Trans_Female;
                case "fluid":
                case "fl":
                    return User.Genders.Fluid;
                case "other":
                case "o":
                    return User.Genders.Other;
                case "none":
                case "n":
                    return User.Genders.None;
                default:
                    return User.Genders.Null;
            }

        }

        public static User.Orientation StringToOrientation(string orientation)
        {
            switch (orientation.ToLower())
            {
                case "straight":
                case "s":
                    return User.Orientation.Straight;
                case "gay":
                case "g":
                    return User.Orientation.Gay;
                case "bisexual":
                case "bi":
                    return User.Orientation.Bi;
                case "asexual":
                case "a":
                    return User.Orientation.Asexual;
                case "pansexual":
                case "pan":
                    return User.Orientation.Pan;
                case "demi":
                case "d":
                    return User.Orientation.Demi;
                case "other":
                case "o":
                    return User.Orientation.Oother;
                default:
                    return User.Orientation.None;
            }

        }
    }
}
