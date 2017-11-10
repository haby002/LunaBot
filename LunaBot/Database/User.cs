﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.Database
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Index(IsUnique = true)]
        public long DiscordId { get; set; }

        public uint Privilege { get; set; }

        public bool TutorialFinished { get; set; }

        public int Level { get; set; }

        public int Xp { get; set; }

        public string Nickname { get; set; }

        public Genders Gender { get; set; }

        public Orientation orientation { get; set; }

        public int Age { get; set; }

        public string Fur { get; set; }

        public string Description { get; set; }

        public string Ref { get; set; }

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
        };
    }
}
