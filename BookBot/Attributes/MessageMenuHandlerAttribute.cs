using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Attributes
{
    internal class MessageMenuHandlerAttribute : Attribute
    {
        public List<string> Commands { get; set; }
        public bool Priority { get; private set; }


        public MessageMenuHandlerAttribute(bool priority, params string[] commands)
        {
            Commands = commands.Select(x => GetNameFromResourse(x)).ToList();
            Priority = priority;
        }
        private static string GetNameFromResourse(string command)
        {
            return ConfigApp.GetSettings<CustomSettings>().GetButton(command);
        }
    }
}
