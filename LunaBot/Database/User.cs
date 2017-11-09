using Newtonsoft.Json;
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

        public string Privilege { get; set; }

        public string Description { get; set; }

        public int Level { get; set; }

        public int Xp { get; set; }

        public int Age { get; set; }

        public string Gender { get; set; }

        public string Ref { get; set; }

        //public enum Genders
        //{
        //    Male,
        //    Female,
        //    Other,
        //};
    }
}
