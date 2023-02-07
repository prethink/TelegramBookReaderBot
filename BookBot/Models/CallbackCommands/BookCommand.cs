using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Models.CallbackCommands
{
    internal class BookCommand
    {
        [JsonProperty("1")]
        public long EntityId { get; set; }
        public BookCommand(long entityId)
        {
            EntityId = entityId;
        }
    }
}
