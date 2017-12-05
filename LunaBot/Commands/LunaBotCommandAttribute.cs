using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot.Commands
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Struct)]
    class LunaBotCommandAttribute : Attribute
    {
        public string[] Aliases { get; set; }

        public string Name { get; set; }

        public LunaBotCommandAttribute(string name, params string[] aliases)
        {
            this.Name = name.ToLower();
            this.Aliases = aliases.Select(x => x.ToLower()).ToArray();
        }
    }
}
