using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.Database
{
    public class Setting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column(TypeName = "VARCHAR")]
        [MaxLength(20)]
        public string SettingName { get; set; }

        public string Value { get; set; }
    }
}
