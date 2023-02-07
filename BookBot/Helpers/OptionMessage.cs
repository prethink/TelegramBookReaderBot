using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace BookBot.Helpers
{
    public class OptionMessage
    {
        public ReplyKeyboardMarkup MenuReplyKeyboardMarkup { get; set; }

        public InlineKeyboardMarkup MenuInlineKeyboardMarkup { get; set; }
        public bool ClearMenu = false;
        public string Message { get; set; }
        public int MessageId { get; set; }
        public bool HasMessage()
        {
            return !string.IsNullOrWhiteSpace(Message);
        }
    }
}
