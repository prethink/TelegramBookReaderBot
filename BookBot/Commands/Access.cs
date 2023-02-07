using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static BookBot.Helpers.Extensions.Step;
using BookBot.Helpers.Extensions;
using BookBot.Attributes;
using BookBot.Commands.Common;
using BookBot.Attributes;

namespace BookBot.Commands
{
    public class Access
    {
        /// <summary>
        /// Не найдена команда
        /// </summary>
        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task CommandMissing(ITelegramBotClient botClient, Update update, string command = "")
        {
            await MainCommand.MainMenu(botClient, update);
        }

        /// <summary>
        /// Отображение сообщения что не хватает прав для того, чтобы использовать данный бот
        /// </summary>
        public static async Task PrivilagesMissing(ITelegramBotClient botClient, Update update)
        {
            string msg = $"У вас не достаточно прав на использование этой команды!";
            await Common.Message.Send(botClient, update, msg);
        }

        /// <summary>
        /// Неверный тип данных
        /// </summary>
        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task IncorrectTypeData(ITelegramBotClient botClient, Update update)
        {
            await Common.Message.Send(botClient, update, $"Неверный тип данных, попробуйте еще раз");
        }
    }
}
