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
        public string Name { get; set; }

        public LunaBotCommandAttribute(string name)
        {
            this.Name = name.ToLower();
        }
    }
}
