using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System;

namespace LunaBot.Database
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        
        public ulong DiscordId { get; set; }

        public Privileges Privilege { get; set; }

        public bool TutorialFinished { get; set; }

        public int warnCount { get; set; }

        public int Level { get; set; }

        public int Xp { get; set; }

        public DateTime LastMessage { get; set; }

        public string Nickname { get; set; }

        public Genders Gender { get; set; }

        public Orientation orientation { get; set; }

        public int Age { get; set; }

        public string Fur { get; set; }

        public string Description { get; set; }

        public string Ref { get; set; }

        public bool Nsfw { get; set; }

        public bool Monk { get; set; }

        public bool Games { get; set; }

        public bool BotUpdates { get; set; }

        public int SnugR { get; set; }

        public int SnugG { get; set; }

        public enum Genders
        {
            Null,
            None,
            Male,
            Female,
            Trans_Female,
            Trans_Male,
            Fluid,
            Other
        };

        public enum Orientation
        {
            None,
            Straight,
            Gay,
            Bi,
            Asexual,
            Pan,
            Demi
        };

        public enum Privileges
        {
            User,
            Moderator,
            Admin,
            Owner
        }
    }
}
