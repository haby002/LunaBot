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
        
        public long DiscordId { get; set; }

        public Privileges Privilege { get; set; }

        public bool TutorialFinished { get; set; }

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

        public enum Genders
        {
            None,
            Male,
            Female,
            TransF,
            TransM,
            Other,
        };

        public enum Orientation
        {
            None,
            Straight,
            Gay,
            Bi,
            Asexual,
            Pansexual,
            Gray
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
