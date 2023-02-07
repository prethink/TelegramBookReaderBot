using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace BookBot.Attributes
{
    internal class RequireDateAttribute : Attribute
    {
        public MessageType TypeData { get; set; }

        public RequireDateAttribute(MessageType typeData)
        {
            TypeData = typeData;
        }
    }
}
