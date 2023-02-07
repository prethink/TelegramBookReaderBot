using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Models.CallbackCommands
{
    public class CallbackBaseCommand
    {
        [JsonProperty("0")]
        public InlineCallbackCommands LastCommand { get; set; }
        public CallbackBaseCommand(InlineCallbackCommands data = InlineCallbackCommands.None)
        {
            LastCommand = data;
        }
    }
}
