using BookBot.Models.DataBase;
using BookBot.Models.DataBase.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BookBot.Helpers.Extensions
{
    public static class Cache
    {
        static Dictionary<long, UserCache> _userHandlerData = new();
        public static void CreateCacheData(this Update update)
        {
            long userId = update.GetChatId();
            ClearCacheData(update);
            _userHandlerData.Add(userId, new UserCache());
        }

        public static UserCache GetCacheData(this Update update)
        {
            long userId = update.GetChatId();
            var data = _userHandlerData.FirstOrDefault(x => x.Key == userId);
            if (data.Equals(default(KeyValuePair<long, UserCache>)))
            {
                CreateCacheData(update);
                return _userHandlerData.FirstOrDefault(x => x.Key == userId).Value;
            }
            else
            {
                return data.Value;
            }
        }

        public static void ClearCacheData(this Update update)
        {
            long userId = update.GetChatId();
            if (HasCacheData(update))
            {
                _userHandlerData.FirstOrDefault(x => x.Key == userId).Value.ClearData();
            }

        }

        public static bool HasCacheData(this Update update)
        {
            long userId = update.GetChatId();
            return _userHandlerData.ContainsKey(userId);
        }
    }

    public class UserCache
    {
        public long? Id { get; set; }
        public Telegram.Bot.Types.Message LastMessage { get; set; }
        public List<Telegram.Bot.Types.Message> MyMessage { get; set; } = new();
        public int PageId { get;  set; }
        public int MaxPageCount { get; internal set; }

        public void ClearData()
        {
            this.LastMessage = null;
            this.MyMessage.Clear();
        }
    }
}
