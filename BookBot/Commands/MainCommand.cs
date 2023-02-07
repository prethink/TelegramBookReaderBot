using BookBot.Attributes;
using BookBot.Commands.Common;
using BookBot.Helpers;
using BookBot.Helpers.Extensions;
using BookBot.Models;
using BookBot.Models.DataBase;
using BookBot.Models.DataBase.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BookBot.Commands
{
    internal class MainCommand
    {
        [MessageMenuHandler(true, nameof(Router.BT_MENU), nameof(Router.BT_MAIN_MENU))]
        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task MainMenu(ITelegramBotClient botClient, Update update)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                    if(user == null)
                    {
                        await RegisterCommand.Start(botClient, update);
                        return;
                    }
                }
                await MainMenu(botClient,  update.GetChatId(), MessagesPattern.GetMessage(nameof(MessagesPattern.MSG_MAIN_MENU)));
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public static async Task MainMenu(ITelegramBotClient botClient, long telegramId)
        {
            try
            {
                await MainMenu(botClient, telegramId, MessagesPattern.GetMessage(nameof(MessagesPattern.MSG_MAIN_MENU)));
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task MainMenu(ITelegramBotClient botClient, long telegramId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "Главное меню";
                }

                using (var db = new AppDbContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(x => x.TelegramId == telegramId);
                    if(user == null)
                    {
                        await Common.Message.Send(botClient, telegramId, "Для начала работы бота, напишите /start");
                        return;
                    }

                    user.AddActivity(3);
                    await db.SaveChangesAsync();
                    var option = new OptionMessage();
                    var menuBotton = new List<string>();
                    
                    if(user.CurrentBookId != null)
                    {
                        menuBotton.Add(Router.GetValueButton(nameof(Router.BT_READ)));
                    }
                    menuBotton.Add(Router.GetValueButton(nameof(Router.BT_BOOKS)));     
                    //menuBotton.Add(Router.GetValueButton(nameof(Router.BT_SETTINGS)));
                    var menuBot = MenuGenerator.ReplyButtons(2, menuBotton);
                    var resultMenu = MenuGenerator.ReplyKeyboard(1,menuBotton, true);
                    option.MenuReplyKeyboardMarkup = resultMenu;
                    await Common.Message.Send(botClient, telegramId, message, option);
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

    }
}
