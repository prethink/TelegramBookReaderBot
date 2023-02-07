using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Attributes
{
    public class SlashCommandAttribute : Attribute
    {
        public List<string> Commands { get; set; }

        public SlashCommandAttribute(params string[] commands)
        {
            Commands = commands.ToList();
        }
    }
}
