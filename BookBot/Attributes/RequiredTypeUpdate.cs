using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace BookBot.Attributes
{
    internal class RequiredTypeUpdateAttribute : Attribute
    {
        public List<ChatType> TypeUpdate { get; set; } = new List<ChatType>();

        public RequiredTypeUpdateAttribute(params ChatType[] types)
        {
            TypeUpdate.AddRange(types.ToList());
        }
    }
}
