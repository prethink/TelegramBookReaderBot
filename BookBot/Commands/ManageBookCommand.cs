using BookBot.Helpers;
using BookBot.Models.CallbackCommands;
using BookBot.Models.DataBase;
using BookBot.Models;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookBot.Attributes;
using BookBot.Models.DataBase.Base;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.EntityFrameworkCore;
using BookBot.Helpers.Extensions;
using BookBot.Models.Readers;

namespace BookBot.Commands
{
    internal class ManageBookCommand
    {
        

        [MessageMenuHandler(true, nameof(Router.BT_SETTINGS_BOOK))]

        public static async Task ManageBook(ITelegramBotClient botClient, Update update)
        {
            try
            {
                await GetMessage(botClient, update.GetChatId(), true);
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }


        public static async Task<string>GetMessage(ITelegramBotClient botClient,long telegramId,bool showNewMessage)
        {
            string result = "-";
            using (var db = new AppDbContext())
            {
                var user = await db.Users.Include(x => x.Settings).FirstOrDefaultAsync(x => x.TelegramId == telegramId);
                if (user != null && user.CurrentBookId != null)
                {
                    var book = await db.Books.FirstOrDefaultAsync(x => x.id == user.CurrentBookId);
                    if (book != null)
                    {
                        var reader = await book.GetBookData();
                        result =
                            $"⚙️ Панель управления" +
                            $"\n📖 {book.Title}" +
                            $"\n📃 Всего страниц: {reader.GetPagesCount()}";

                        var settings = user.Settings.FirstOrDefault(x => x.BookId == book.id);
                        if (settings != null)
                        {
                            result += $"\n\n📃 Вы на странице: {settings.CurrentPage}";

                            result += "\n\nУровни вложенности содержания\n 1 уровень - 🟫\n 2 уровень - ◻️\n 3 уровень - ◾";


                            if (settings.IsRepeat)
                            {
                                result += $"\n🔔 Выдача новых страниц: Включена" +
                                    $"\n⏱️ Периодичность: {settings.RepeatTime.ToReadableString()}" +
                                    $"\n📅 Дата следующего уведомления: {settings.NextNotifyTime.ToString("dd.MM.yyyy HH:mm")}";
                            }
                            else
                            {
                                result += "\n🔔 Выдача новых страниц: Выключена";
                            }

                            result += "\n\n❓ Справка:" +
                                "\n🔔 При включенных уведомления бот с определенной периодичностью будет присылать новые страницы для чтения вашей книги c того места, с которого вы остановились последний раз.\n❗ Если уведомления вам мешают, вы всегда можете их отключить";

                            if (showNewMessage)
                            {
                                await Common.Message.Send(botClient, telegramId, result, GetManageBookMenu(book.id));
                            }
                        }
                        reader.Dispose();
                    }
                }
            }

            return result;
        }
        public static OptionMessage GetManageBookMenu(long bookId)
        {
            var option = new OptionMessage();
            var changeChapterButton = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_SETTINGS_BOOK_CHAPTERS)), InlineCallbackCommands.BookChangeChapter, new ChapterCommand(bookId,1,ChapterCommand.PAGE_SIZE,0));
            var changePageButton = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_SETTINGS_BOOK_PAGE)), InlineCallbackCommands.BookChangePage, new EntityCommand(bookId));
            var notifyButton = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_SETTINGS_BOOK_NOTIFY)), InlineCallbackCommands.BookNotify, new EntityCommand(bookId));
            option.MenuInlineKeyboardMarkup = MenuGenerator.InlineKeyboard(1, new List<IInlineContent>() { changeChapterButton, changePageButton, notifyButton });
            return option;
        }

        [InlineCallbackHandler(InlineCallbackCommands.BookChangeChapter)]
        public static async Task ChangeChapter(ITelegramBotClient botClient, Update update)
        {
            try 
            {
                var command = InlineCallbackCommand<ChapterCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if(command != null)
                {
                    var option = new OptionMessage();
                    var backButton = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_BACK_BUTTON)), InlineCallbackCommands.BookNotifyBack, new EntityCommand(command.Data.BookId));
                    var buttons = new List<IInlineContent>();
                    buttons.Add(backButton);
                    var chapters = await GetChapters(update.GetChatId(), command.Data.BookId, command.Data.Page);
                    foreach (var item in chapters.Results)
                    {
                        string addSymbol = "🟫 ";
                        if(item.IsSubSub())
                        {
                            addSymbol = "◾ ";
                        }
                        else if(item.IsSub())
                        {
                            addSymbol = "◻️ ";
                        }
                        var chapterButton = new InlineCallbackCommand(addSymbol + item.GetName() ?? "?", InlineCallbackCommands.BookSelectChapter, new ChapterCommand(command.Data.BookId,command.Data.Page,ChapterCommand.PAGE_SIZE,item.GetChapterId()));
                        buttons.Add(chapterButton);

                    }


                    var manageButtons = new List<IInlineContent>();
                    if (command.Data.Page > 1)
                    {
                        var prevButton = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_PREV_PAGE)), InlineCallbackCommands.BookChangeChapter, new ChapterCommand(command.Data.BookId, command.Data.Page - 1, ChapterCommand.PAGE_SIZE, 0));
                        manageButtons.Add(prevButton);
                    }

                    if (command.Data.Page < chapters.PageCount)
                    {
                        var nextButton = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_NEXT_PAGE)), InlineCallbackCommands.BookChangeChapter, new ChapterCommand(command.Data.BookId, command.Data.Page +1, ChapterCommand.PAGE_SIZE, 0));
                        manageButtons.Add(nextButton);
                    }
                    var buttonPack = MenuGenerator.InlineButtons(1, buttons);
                    var buttonsPackTwo = MenuGenerator.InlineButtons(2, manageButtons);
                    buttonPack.AddRange(buttonsPackTwo);

                    option.MenuInlineKeyboardMarkup = MenuGenerator.InlineKeyboard(buttonPack);
                    await Common.Message.EditInline(botClient, update.GetChatId(), update.CallbackQuery.Message.MessageId, option);
                }
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [InlineCallbackHandler(InlineCallbackCommands.BookSelectChapter)]
        public static async Task SelectChapter(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<ChapterCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if (command != null)
                {
                    using(var db = new AppDbContext())
                    {
                        var user = await db.Users.Include(x => x.Books).FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                        if(user != null)
                        {
                            var book = user.Books.FirstOrDefault(x => x.id == command.Data.BookId);
                            if(book != null)
                            {
                                var reader = await book.GetBookData();
                                var page = reader.GetPageByChapter((int)command.Data.SelectedChapter);
                                await BookCommand.ReadByPage(botClient, update.GetChatId(), page, book.id);
                                reader.Dispose();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [InlineCallbackHandler(InlineCallbackCommands.BookChangeChapterHandler)]
        public static async Task ChangeChapterHandler(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<ChapterCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if (command != null)
                {
                    
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public static async Task<PagedResult<IChapter>> GetChapters(long telegramId,long bookId,long page)
        {

            using(var db = new AppDbContext())
            {
                var user = await db.Users.Include(x => x.Books).FirstOrDefaultAsync(x => x.TelegramId == telegramId);
                if(user != null)
                {
                    var needBook = user.Books.FirstOrDefault(x => x.id == bookId);
                    if(needBook != null)
                    {
                        var reader = await needBook.GetBookData();
                        var result = await reader.GetChapters().GetPaged<IChapter>((int)page, (int)ChapterCommand.PAGE_SIZE);
                        reader.Dispose();
                        return result;

                    }
                }
            }

            return new PagedResult<IChapter>()
            {
                PageCount = 0,
                PageSize = 0,
                Results = new List<IChapter>(),
                CurrentPage = (int)page,
                RowCount = 0,
            };
        }


        [InlineCallbackHandler(InlineCallbackCommands.BookNotify)]
        public static async Task NotifyBookSettings(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if(command != null)
                {
                    using (var db = new AppDbContext())
                    {
                        var user = await db.Users.Include(x => x.Settings).FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                        if(user != null)
                        {
                            var searchSettings = user.Settings.FirstOrDefault(x => x.BookId == command.Data.EntityId);
                            if(searchSettings != null)
                            {
                                string changeStateButtonName = searchSettings.IsRepeat ? "🔔 Выключить" :"🔔 Включить";
                                string repeateMsg = "⏱️ Переодичность уведомления";
                                var notifyButton = new InlineCallbackCommand(changeStateButtonName, InlineCallbackCommands.BookNotifyChangeState, new EntityCommand(command.Data.EntityId));
                                var changeRepeatTime = new InlineCallbackCommand(repeateMsg, InlineCallbackCommands.BookChangeRepeatTime, new EntityCommand(command.Data.EntityId));
                                var backButton = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_BACK_BUTTON)), InlineCallbackCommands.BookNotifyBack, new EntityCommand(command.Data.EntityId));
                                var option = new OptionMessage();
                                option.MenuInlineKeyboardMarkup = MenuGenerator.InlineKeyboard(1,new List<IInlineContent> {notifyButton, changeRepeatTime, backButton });
                                await Common.Message.EditInline(botClient, update.GetChatId(), update.CallbackQuery.Message.MessageId, option);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }


        [InlineCallbackHandler(InlineCallbackCommands.BookNotifyChangeState)]
        public static async Task NotifyBookHandler(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if (command != null)
                {
                    using (var db = new AppDbContext())
                    {
                        var user = await db.Users.Include(x => x.Settings).FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                        if (user != null)
                        {
                            var searchSettings = user.Settings.FirstOrDefault(x => x.BookId == command.Data.EntityId);
                            if (searchSettings != null)
                            {
                                searchSettings.IsRepeat = !searchSettings.IsRepeat;
                                await db.SaveChangesAsync();
                                var msg = await GetMessage(botClient, update.GetChatId(), false);
                                string changeStateButtonName = searchSettings.IsRepeat ? "🔔 Выключить" : "🔔 Включить";
                                string repeateMsg = "⏱️ Переодичность уведомления";
                                var notifyButton = new InlineCallbackCommand(changeStateButtonName, InlineCallbackCommands.BookNotifyChangeState, new EntityCommand(command.Data.EntityId));
                                var changeRepeatTime = new InlineCallbackCommand(repeateMsg, InlineCallbackCommands.BookChangeRepeatTime, new EntityCommand(command.Data.EntityId));
                                var backButton = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_BACK_BUTTON)), InlineCallbackCommands.BookNotifyBack, new EntityCommand(command.Data.EntityId));
                                var option = new OptionMessage();
                                option.MenuInlineKeyboardMarkup = MenuGenerator.InlineKeyboard(1, new List<IInlineContent> { notifyButton, changeRepeatTime, backButton });
                                await Common.Message.Edit(botClient, update.GetChatId(), update.CallbackQuery.Message.MessageId, msg, option);
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [InlineCallbackHandler(InlineCallbackCommands.BookNotifyBack)]
        public static async Task NotifyBack(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if(command != null)
                {
                    await Common.Message.EditInline(botClient, update.GetChatId(), update.CallbackQuery.Message.MessageId, GetManageBookMenu(command.Data.EntityId));
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [InlineCallbackHandler(InlineCallbackCommands.BookChangePage)]
        public static async Task ChangePage(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if(command != null)
                {
                    using (var db = new AppDbContext())
                    {
                        var user = await db.Users.FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                        if(user != null)
                        {
                            bool needSave = false;
                            if (user?.CurrentBookId != command.Data.EntityId)
                            {
                                needSave = true;
                                user.CurrentBookId = command.Data.EntityId;
                            }

                            var book = await db.Books.FirstOrDefaultAsync(x => x.id == user.CurrentBookId);
                            if (book != null)
                            {
                                var reader = await book.GetBookData();
                                string msg = $"Укажите номер страницы книги, чтобы перейти к ней\nДопустимый диапазон 1-{reader.GetPagesCount()}";
                                update.GetCacheData().MaxPageCount = reader.GetPagesCount();
                                await Common.Message.Edit(botClient, update, msg);
                                update.RegisterNextStep(new CommandStep() { CommandDelegate = ChangePageHandler });
                                reader.Dispose();
                            }

                            if(needSave)
                            {
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }



        [RequireDate(Telegram.Bot.Types.Enums.MessageType.Text)]
        public static async Task ChangePageHandler(ITelegramBotClient botClient, Update update)
        {
            try
            {
                if(int.TryParse(update.Message.Text, out var pageCount))
                {
                    var maxCount = update.GetCacheData().MaxPageCount + 1;
                    if (pageCount > 0 && pageCount <= maxCount)
                    {
                        await BookCommand.ReadByPage(botClient, update.GetChatId(), pageCount - 1);
                        update.ClearStepUser();
                    }
                    else
                    {
                        if(pageCount < 1)
                        {
                            string errMsg = "Значение должно быть больше 0";
                            await Common.Message.Send(botClient, update, errMsg);
                        }
                        else if (pageCount > update.GetCacheData().MaxPageCount)
                        {
                            string errMsg =  $"Значение должно быть меньше или равно {maxCount}";
                            await Common.Message.Send(botClient, update, errMsg);
                        }
                    }
                }
                else
                {
                    string errMsg = "Значение должно быть числом, попробуйте еще раз";
                    await Common.Message.Send(botClient,update, errMsg);    
                }

            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [InlineCallbackHandler(InlineCallbackCommands.BookChangeRepeatTime)]
        public static async Task ChangeRepeatTime(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if (command != null)
                {
                    using (var db = new AppDbContext())
                    {
                        var user = await db.Users.FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                        if (user != null)
                        {
                            bool needSave = false;
                            if (user?.CurrentBookId != command.Data.EntityId)
                            {
                                needSave = true;
                                user.CurrentBookId = command.Data.EntityId;
                            }

                            var book = await db.Books.FirstOrDefaultAsync(x => x.id == user.CurrentBookId);
                            if (book != null)
                            {
                                var reader = await book.GetBookData();
                                string msg = $"Укажите с какой переодичностью должны приходить новые уведомления для данной книги, значение указывается в минутах\nДопустимый диапозон {SettingsUser.MIN_REPEAT_TIME}-{SettingsUser.MAX_REPEAT_TIME}";
                                update.GetCacheData().MaxPageCount = reader.GetPagesCount();
                                await Common.Message.Edit(botClient, update, msg);
                                update.RegisterNextStep(new CommandStep() { CommandDelegate = ChangeRepeatTimeHandler });
                                reader.Dispose();
                            }

                            if (needSave)
                            {
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [RequireDate(Telegram.Bot.Types.Enums.MessageType.Text)]
        public static async Task ChangeRepeatTimeHandler(ITelegramBotClient botClient, Update update)
        {
            try
            {
                if (int.TryParse(update.Message.Text, out var minut))
                {
                    if (minut >=  SettingsUser.MIN_REPEAT_TIME && minut <= SettingsUser.MAX_REPEAT_TIME)
                    {
                        using(var db = new AppDbContext())
                        {
                            var user = await db.Users.Include(x => x.Settings).FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                            if(user != null)
                            {
                                if(user.CurrentBookId != null)
                                {
                                    var settings = user.Settings.FirstOrDefault(x => x.BookId == user.CurrentBookId);
                                    if(settings != null)
                                    {
                                        settings.RepeatTime = TimeSpan.FromMinutes(minut);
                                        settings.NextNotifyTime = DateTime.Now.AddMinutes(minut);
                                        await db.SaveChangesAsync();
                                        string msg = $"Переодичность уведомлений: {settings.RepeatTime.ToReadableString()}\nСледующее уведомление: {settings.NextNotifyTime.ToString("dd.MM.yyyy HH:mm")}";
                                        await Common.Message.Send(botClient, update, msg);
                                    }
                                }
                            }
                        }
                        update.ClearStepUser();
                    }
                    else
                    {
                        if (minut <  SettingsUser.MIN_REPEAT_TIME)
                        {
                            string errMsg = $"Значение должно быть больше {SettingsUser.MIN_REPEAT_TIME}";
                            await Common.Message.Send(botClient, update, errMsg);
                        }
                        else if (minut > SettingsUser.MAX_REPEAT_TIME)
                        {
                            string errMsg = $"Значение должно быть меньше или равно {SettingsUser.MAX_REPEAT_TIME}";
                            await Common.Message.Send(botClient, update, errMsg);
                        }
                    }
                }
                else
                {
                    string errMsg = "Значение должно быть числом, попробуйте еще раз";
                    await Common.Message.Send(botClient, update, errMsg);
                }

            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }


    }
}
