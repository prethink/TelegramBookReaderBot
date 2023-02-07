using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BookBot.Helpers
{
    internal class GroupHelper
    {
        public static async Task<bool> IsMemberGroup(ITelegramBotClient botClient, long groupId, long userId)
        {
            try
            {
                var data = await botClient.GetChatMemberAsync(groupId, userId);
                return  data.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Member ||
                        data.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Administrator ||
                        data.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Creator ||
                        data.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Administrator;
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return false;
            }
        }
    }
}
