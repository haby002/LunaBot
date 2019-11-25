using Discord;

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
        internal static ulong[] Owners =
        {
            123470919535427584,
            201934665961963520,
            196558107520794624
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

        /// <summary>
        /// Approval and disapproval of new users
        /// </summary>
        internal static ulong ProvingGrounds = 547485847424073739;

    }

    internal static class Categories
    {
        internal static ulong IntroRooms = 547485651533299743;
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

        internal static string Moddlet = "Mod";

        internal static string Staff = "Staff";
        internal static ulong StaffId = 648408054127394837;

        internal static string Newbie = "Newbie";
        internal static ulong NewbieId = 549391161010159616;

        internal static string SFW = "SFW";

        internal static string Monk = "Monk";

        internal static string Verified = "Verified";
        internal static ulong VerifiedId = 485485460160905228;

        // Orientation Roles
        internal static string Gay = "gay";

        internal static string Straight = "straight";

        internal static string Asexual = "asexual";

        internal static string Bi = "bi";

        internal static string Pan = "pan";
        
        internal static string Demi = "demi";

        internal static string Fluid = "fluid";

        // Gender Roles
        internal static string Male = "male";

        internal static string Female = "female";

        internal static string TransMale = "trans-male";

        internal static string TransFemale = "trans-female";

        internal static string Other = "other";
        
        internal static string None = "none";

        // Moderation Roles
        internal static string Mute = "Muted";

        // Announcement Roles
        internal static string Games = "Games";

        internal static string BotUpdates = "Bot Updates";

        internal static string OpenDM = "Open DM";

    }

    internal static class Permissions
    {
        internal static OverwritePermissions removeAllPerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

        internal static OverwritePermissions userPerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

        internal static OverwritePermissions adminPerm = new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow);

        internal static OverwritePermissions roomPerm = new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit);

        internal static OverwritePermissions mutePerm = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

    }

    internal static class ReportColors
    {
        internal static Color userCommand = Color.Purple;

        internal static Color modCommand = Color.DarkMagenta;

        internal static Color adminCommand = Color.Magenta;

        internal static Color ownerCommand = Color.Gold;

        internal static Color userJoined = Color.Teal;

        internal static Color userLeft = Color.Orange;

        internal static Color spamBlock = Color.Red;

        internal static Color userKicked = Color.DarkOrange;

        internal static Color userBanned = Color.DarkRed;

        internal static Color exception = Color.Red;
    }

    internal static class BannedWords
    {
        internal static string[] words =
        {
            "nigger",
            "nig",
            "niger",
            "faggot",
            "fagot",
            "fag",
            "fagoot",
            "cunt"
        };
    }
}
