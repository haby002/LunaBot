using Discord;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Stores constant like user, channel, and guild IDs
/// </summary>
namespace LunaBot.ServerUtilities
{
    internal static class UserIds
    {
        /// <summary>
        /// Owners of the FR server
        /// </summary>
        internal static ulong[] Mods =
        {
            //Doodles
            285606103243554816,
            //Rand
            353124842540761089,
            //FireFlash
            92466867364433920,
            //Jason
            263506098768707595,
            //Hellblaze Wolf
            180623286747660288,
            //Zelenyy (TEMP FOR TESTING)
            284861595396472834
        };

        /// <summary>
        /// The bot's ID
        /// </summary>
        internal static ulong Luna = 333285108402487297;

    }

    internal static class Channels
    {
        /// <summary>
        /// FR lobby channel. Main channel
        /// </summary>
        internal static ulong Lobby = 308306400717832192;

        /// <summary>
        /// Channel for error reporting and status reports
        /// </summary>
        internal static ulong BotLogs = 379784655370584074;

    }

    internal static class Guilds
    {
        /// <summary>
        /// FR guild
        /// </summary>
        internal static ulong Guild = 195198580724727810;

    }

    internal static class Roles
    {
        //Permission Roles
        internal static string Owner = "Owner";

        internal static string Admin = "Admin";

        internal static string Moddlet = "Moddlet";

        internal static string Staff = "Staff";

        internal static string SFW = "SFW";

        internal static string Monk = "Monk";

        // Orientation Roles
        internal static string Gay = "gay";

        internal static string Straight = "straight";

        internal static string Asexual = "asexual";

        internal static string Bi = "bi";

        internal static string Pan = "pan";

        internal static string GrayA = "grey-a";

        // Gender Roles
        internal static string Male = "male";

        internal static string Female = "female";

        internal static string TransMale = "trans-male";

        internal static string TransFemale = "trans-female";

        internal static string Other = "other";

        // Moderation Roles
        internal static string Mute = "Mute";

    }

    internal static class Permissions
    {
        internal static OverwritePermissions removeAllPerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

        internal static OverwritePermissions userPerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

        internal static OverwritePermissions lunaTutPerm = new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow);

        internal static OverwritePermissions roomPerm = new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit);

        internal static OverwritePermissions mutePerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

    }
}
