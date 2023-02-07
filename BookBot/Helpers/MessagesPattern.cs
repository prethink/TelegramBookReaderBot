using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Helpers
{
    public class MessagesPattern
    {
        public const string MSG_AFTER_REGISTER              = "1m";
        public const string MSG_ALREADY_REGISTERED          = "2m";
        public const string MSG_NEW_USER                    = "3m";
        public const string MSG_NOT_HAVE_USERS              = "4m";
        public const string MSG_NOTIFY_NEW_LIKE             = "5m";
        public const string MSG_MAIN_MENU                   = "6m";




        public static string GetMessage(string messagePattern)
        {
            return ConfigApp.GetSettings<CustomSettings>().GetMessage(messagePattern);
        }
    }
}
