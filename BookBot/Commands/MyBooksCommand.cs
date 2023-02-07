using BookBot.Attributes;
using BookBot.Helpers;
using BookBot.Helpers.Extensions;
using BookBot.Models.CallbackCommands;
using BookBot.Models;
using BookBot.Models.DataBase.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using BookBot.Models.DataBase;
using BookBot.Models.Readers;

namespace BookBot.Commands
{
    internal class MyBooksCommand
    {
        [MessageMenuHandler(true,nameof(Router.BT_MYBOOKS))]
        public static async Task MyBooks(ITelegramBotClient botClient,Update update)
        {
            try
            {
                await LoadBook(botClient, update, 0,true);
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public static async Task LoadBook(ITelegramBotClient botClient, Update update, int bookId,bool isNewMessage)
        {
            try
            {
                var pathBook = FileWorker.PathBook;
                using(var db = new AppDbContext())
                {
                    var user = await db.Users.Include(x => x.Books).FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                    if(user.Books.Count == 0)
                    {
                        string msg = "Вы еще не добавили не одной книги";
                        if(isNewMessage)
                        {
                            await Common.Message.SendPhoto(botClient, update.GetChatId(), msg, pathBook);
                        }
                        else
                        {
                            await Common.Message.EditPhoto(botClient, update.GetChatId(), update.GetMessageId(), pathBook);
                            await Common.Message.EditCaption(botClient, update, msg);
                        }

                        return;
                    }

                    if(bookId > user.Books.Count - 1 )
                    {
                        string msg = "Больше нет книг";
                        if (isNewMessage)
                        {
                            await Common.Message.SendPhoto(botClient, update.GetChatId(), msg, pathBook);
                        }
                        else
                        {
                            await Common.Message.EditPhoto(botClient, update.GetChatId(),update.GetMessageId(), pathBook);
                            await Common.Message.EditCaption(botClient, update, msg);
                        }
                        return;
                    }

                    var book = user.Books[bookId];
                    if(book != null)
                    {
                        var option = new OptionMessage();
                        var listManage = new List<IInlineContent>();
                        var listReadBook = new List<IInlineContent>();
                        var myBookNextBt = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_NEXT_PAGE)), InlineCallbackCommands.MyBookNext, new EntityCommand(bookId + 1));
                        var myBookPrevBt = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_PREV_PAGE)), InlineCallbackCommands.MyBookPrev, new EntityCommand(bookId - 1));
                        var myBookReadBt = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_READ)), InlineCallbackCommands.MyBookRead, new EntityCommand(book.id));
                        var myBookDownload = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_DOWNLOAD_BUTTON)), InlineCallbackCommands.MyBookDownload, new EntityCommand(book.id));
                        var myBookDelete = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_DELETE_BUTTON)), InlineCallbackCommands.MyBookDelete, new EntityCommand(book.id));

                        if(bookId > 0)
                        {
                            listManage.Add(myBookPrevBt);
                        }

                        if(bookId < user.Books.Count - 1)
                        {
                            listManage.Add(myBookNextBt);
                        }

                        if(user.CurrentBookId == null || user.CurrentBookId != book.id)
                        {
                            listReadBook.Add(myBookReadBt);
                        }
                        listReadBook.Add(myBookDownload);
                        listReadBook.Add(myBookDelete);

                        var inlineButtons = MenuGenerator.InlineButtons(2, listManage);
                        var inlineButtonsTwo = MenuGenerator.InlineButtons(1, listReadBook);

                        inlineButtons.AddRange(inlineButtonsTwo);

                        option.MenuInlineKeyboardMarkup = MenuGenerator.InlineKeyboard(inlineButtons);
                        try
                        {
                            var bookreader = await book.GetBookData();
                            if(!isNewMessage)
                            {
                                option.MessageId = update.GetMessageId();
                            }
                            await bookreader.ShowTitle(botClient, update.GetChatId(), option, !isNewMessage);
                            bookreader.Dispose();
                        }
                        catch(Exception ex)
                        {
                            TelegramService.GetInstance().InvokeErrorLog(ex);
                            
                            string errorMsg = $"{Router.S_WARNING} Файл книги не может быть обработан\n";
                            if (isNewMessage)
                            {

                                await Common.Message.SendPhoto(botClient, update.GetChatId(), errorMsg + book.Title, pathBook, option);
                            }
                            else
                            {
                                await Common.Message.EditPhoto(botClient, update.GetChatId(), update.GetMessageId(), errorMsg + pathBook, option);
                                await Common.Message.EditCaption(botClient, update, errorMsg + book.Title, option);
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

        [InlineCallbackHandler(InlineCallbackCommands.MyBookNext, InlineCallbackCommands.MyBookPrev)]
        public static async Task MyBookNextPrev(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if(command != null)
                {
                    await LoadBook(botClient, update, (int)command.Data.EntityId, false);
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [InlineCallbackHandler(InlineCallbackCommands.MyBookDownload)]
        public static async Task MyBookDownload(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if (command != null)
                {
                    using(var db = new AppDbContext())
                    {
                        var book = await db.Books.FirstOrDefaultAsync(x => x.id == command.Data.EntityId);
                        if(book != null)
                        {
                            await Commands.Common.Message.SendFile(botClient, update.GetChatId(), "-", FileWorker.BaseDir + book.Path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [InlineCallbackHandler(InlineCallbackCommands.MyBookDelete)]
        public static async Task DeleteMyBook(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if (command != null)
                {
                    using (var db = new AppDbContext())
                    {
                        var user = await db.Users.Include(x => x.Books).FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                        if (user != null)
                        {
                            var book = user.Books.FirstOrDefault(x => x.id == command.Data.EntityId);
                            if (book != null)
                            {
                                user.Books.RemoveAll(x => x.id == command.Data.EntityId);
                                if(user.CurrentBookId == book.id)
                                {
                                    user.CurrentBookId = null;
                                }
                                string msg = $"Из списка ваших книг удалена:\n{book.Title}";
                                await Common.Message.EditCaption(botClient, update, msg);
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


        [InlineCallbackHandler(InlineCallbackCommands.MyBookRead)]
        public static async Task ReadBook(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if (command != null)
                {
                    using(var db = new AppDbContext())
                    {
                        var user = await db.Users.Include(x => x.Settings).FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                        if(user != null)
                        {
                            var book = await db.Books.FirstOrDefaultAsync(x => x.id == command.Data.EntityId);
                            if(book != null)
                            {
                                user.CurrentBookId = book.id;
                                string msg = $"Теперь вы читаете\n{book.Title}";
                                await Common.Message.EditCaption(botClient,update,msg);
                                if(!user.Settings.Any(x => x.BookId == book.id))
                                {
                                    var settings = SettingsUser.CreateSettings(user.TelegramId, book.id);
                                    user.Settings.Add(settings);
                                }
                                else
                                {
                                    var settings = user.Settings.FirstOrDefault(x => x.BookId == book.id);
                                    settings.NextNotifyTime = settings.NextNotifyTime.Add(settings.RepeatTime);
                                }
                                await MainCommand.MainMenu(botClient, update);
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
    }
}
