using BookBot.Attributes;
using BookBot.Helpers;
using BookBot.Helpers.Extensions;
using BookBot.Models.CallbackCommands;
using BookBot.Models;
using BookBot.Models.DataBase;
using BookBot.Models.DataBase.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;
using BookBot.Models.Readers;
using File = System.IO.File;

namespace BookBot.Commands
{
    internal class BookCommand
    {
        [MessageMenuHandler(true, nameof(Router.BT_READ))]
        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task ReadBook(ITelegramBotClient botClient, Update update)
        {
            try
            {
                await ReadBook(botClient, update.GetChatId(), false);
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public static async Task ReadBook(ITelegramBotClient botClient,long telegramId, bool prevPage)
        {
            using (var db = new AppDbContext())
            {
                var user = await db.Users.Include(x => x.Settings).FirstOrDefaultAsync(x => x.TelegramId == telegramId);
                if (user == null)
                {
                    await MainCommand.MainMenu(botClient, telegramId);
                    return;
                }

                if (user.CurrentBookId == null)
                {
                    await BookMenu(botClient, telegramId);
                    return;
                }

                var settings = user.Settings.FirstOrDefault(x => x.BookId == user.CurrentBookId);
                if (settings == null)
                {
                    settings = SettingsUser.CreateSettings(user.TelegramId, user.CurrentBookId.Value);
                    user.Settings.Add(settings);
                    await db.SaveChangesAsync();
                }

                if (prevPage)
                {
                    settings.CurrentPage = settings.CurrentPage - 2;
                    if (settings.CurrentPage < 0)
                        settings.CurrentPage = 0;

                    await db.SaveChangesAsync();
                }

                await ReadByPage(botClient, telegramId, settings.CurrentPage);
            }
        }

        public static async Task ReadByPage(ITelegramBotClient botClient, long telegramId, int page, long? bookId = null)
        {
            using(var db = new AppDbContext())
            {
                var user = await db.Users.Include(x => x.Settings).Include(x => x.Books).FirstOrDefaultAsync(x => x.TelegramId == telegramId);
                if (user == null)
                {
                    await MainCommand.MainMenu(botClient, telegramId);
                    return;
                }

                if(bookId != null)
                {
                    var needBook = user.Books.FirstOrDefault(x => x.id == bookId);
                    if(needBook != null)
                    {
                        user.CurrentBookId = needBook.id;
                    }
                }

                if (user.CurrentBookId == null)
                {
                    await BookMenu(botClient, telegramId);
                    return;
                }

                var settings = user.Settings.FirstOrDefault(x => x.BookId == user.CurrentBookId);
                if (settings == null)
                {
                    settings = SettingsUser.CreateSettings(user.TelegramId, user.CurrentBookId.Value);
                    user.Settings.Add(settings);
                    await db.SaveChangesAsync();
                }

                var book = await db.Books.FirstOrDefaultAsync(x => x.id == user.CurrentBookId);
                if (book != null)
                {
                    settings.CurrentPage = page;
                    var readerBook = await book.GetBookData();

                    
                    settings.CurrentPage++;
                    settings.NextNotifyTime = DateTime.Now.Add(settings.RepeatTime);

                    var option = new OptionMessage();
                    var menu = new List<string>();
                    if (settings.CurrentPage > 1)
                    {
                        menu.Add(Router.GetValueButton(nameof(Router.BT_PREV_PAGE)) + $" ({settings.CurrentPage - 1})");
                    }
                    if(!(settings.CurrentPage >= readerBook.GetPagesCount()))
                    {
                        menu.Add(Router.GetValueButton(nameof(Router.BT_NEXT_PAGE)) + $" ({settings.CurrentPage + 1})");
                    }



                    var manageBookMenuList = new List<string>() { Router.GetValueButton(nameof(Router.BT_SETTINGS_BOOK)) };
                    var manageBookMenu = MenuGenerator.ReplyButtons(1, manageBookMenuList);
                    var menuPrevNext = MenuGenerator.ReplyButtons(2, menu);
                    menuPrevNext.AddRange(manageBookMenu);
                    option.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(menuPrevNext, true, Router.GetValueButton(nameof(Router.BT_MAIN_MENU)));

                    await readerBook.ShowContent(botClient, telegramId, settings.CurrentPage, option);

                    if (settings.CurrentPage >= readerBook.GetPagesCount())
                    {
                        if (settings.CurrentPage > readerBook.GetPagesCount())
                        {
                            settings.CurrentPage--;
                            settings.IsRepeat = false;
                            await Common.Message.Send(botClient, telegramId, "Похоже это конец вашей книги, вы все прочитали :)", option);
                            await db.SaveChangesAsync();
                            return;
                        }
                    }

                    readerBook.Dispose();
                    await db.SaveChangesAsync();
                }
            }
        }



        [MessageMenuHandler(true, nameof(Router.BT_NEXT_PAGE))]
        public static async Task NextPage(ITelegramBotClient botClient, Update update)
        {
            try
            {
                await ReadBook(botClient, update.GetChatId(), false);
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [MessageMenuHandler(true, nameof(Router.BT_PREV_PAGE))]
        public static async Task PrevPage(ITelegramBotClient botClient, Update update)
        {
            try
            {
                await ReadBook(botClient, update.GetChatId(), true);
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }


        [MessageMenuHandler(true, nameof(Router.BT_ADD_BOOK))]
        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task Books(ITelegramBotClient botClient, Update update)
        {
            try
            {
                using(var db = new AppDbContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                    if(user == null)
                    {
                        await MainCommand.MainMenu(botClient, update);
                        return;
                    }

                    await LoadNewBook(botClient, update);
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [MessageMenuHandler(true, nameof(Router.BT_BOOKS))]
        public static async Task BookMenu(ITelegramBotClient botClient, Update update)
        {
            try
            {
                await BookMenu(botClient, update.GetChatId());
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }
        public static async Task BookMenu(ITelegramBotClient botClient, long telegramId)
        {
            try
            {
                string msg = "📚 Меню работы с книгами";
                var option = new OptionMessage();
                var menu = new List<string>();
                using(var db = new AppDbContext())
                {
                    var user = await db.Users.Include(x => x.Books).FirstOrDefaultAsync(x => x.TelegramId == telegramId);
                    menu.Add(Router.GetValueButton(nameof(Router.BT_ADD_BOOK)));
                    if(user?.Books?.Count > 0)
                    {
                        menu.Add(Router.GetValueButton(nameof(Router.BT_MYBOOKS)) + $" ({user?.Books?.Count})");
                    }
                }
                option.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, menu, true, Router.GetValueButton(nameof(Router.BT_MAIN_MENU)));
                await Common.Message.Send(botClient, telegramId, msg, option);
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public static async Task LoadNewBook(ITelegramBotClient botClient, Update update)
        {
            try
            {
                string msg = "📗 Загрузите книгу или сразу несколько книг формата:\n ◽ fb2\n ◽ pdf\n\n" +
                    $"{Router.S_WARNING} Максимальный размер 20мб";
                var option = new OptionMessage();
                option.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, new List<string>(), true, Router.GetValueButton(nameof(Router.BT_MAIN_MENU)));
                await Common.Message.Send(botClient,update,msg, option);
                update.RegisterNextStep(new CommandStep() { CommandDelegate = LoadNewBookHandler });
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }



        [RequireDate(Telegram.Bot.Types.Enums.MessageType.Document)]
        public static async Task LoadNewBookHandler(ITelegramBotClient botClient, Update update)
        {
            string filePath = "";
            try
            {
                if(update.Message.Document.FileName.Contains(".fb2") || update.Message.Document.FileName.Contains(".pdf"))
                {
                    using(var db = new AppDbContext())
                    {
                        var user = await db.Users.Include(x => x.Settings).Include(x => x.Books).FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                        if(user == null)
                        {
                            update.ClearStepUser();
                            await MainCommand.MainMenu(botClient, update);
                            return;
                        }
                        var booksCount = db.Books.Count();
                        var telegramFile = await Helpers.FileWorker.DownloadFile(botClient, update.GetChatId(), update.Message.Document.FileId, booksCount.ToString() + "-" + update.Message.Document.FileName);
                        filePath = FileWorker.BaseDir + telegramFile;
                        var Hash = HashHelper.HashFile(telegramFile);
                        var book = await db.Books.FirstOrDefaultAsync(x => x.Hash == Hash);
                        if(book == null)
                        {
                            var reader = await Reader.LoadBook(telegramFile);
                            var isValid = reader.Validation();
                            if(!isValid)
                            {
                                await Commands.Common.Message.Send(botClient, update.GetChatId(), $"{Router.S_WARNING} Файл поврежден или не валидный, попробуйте другой файл.");
                                if (File.Exists(filePath))
                                {
                                    File.Delete(filePath);
                                }
                                return;
                            }
                            var newBook = new Book();
                            newBook.Title = reader.GetTitle();
                            newBook.Path = telegramFile;
                            newBook.Authors = new List<Author>();
                            newBook.Genres = new List<Genre>();
                            newBook.Hash = Hash;

                            foreach (var genre in reader.GetGenres())
                            {
                                var resGenre = await db.Genres.FirstOrDefaultAsync(x => x.Name == genre);
                                if (resGenre != null)
                                {
                                    newBook.Genres.Add(resGenre);
                                }
                                else
                                {
                                    newBook.Genres.Add(new Genre() { Name = genre });
                                }
                            }

                            foreach (var author in reader.GetAuthors())
                            {
                                var resAuthor = await db.Authors.FirstOrDefaultAsync(x => x.Name == author);
                                if (resAuthor != null)
                                {
                                    newBook.Authors.Add(resAuthor);
                                }
                                else
                                {
                                    newBook.Authors.Add(new Author() { Name = author });
                                }
                            }
                            db.Add(newBook);
                            await db.SaveChangesAsync();
                            book = newBook;
                            reader.Dispose();
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        }

                        var settings = user.Settings.FirstOrDefault(x => x.BookId == book.id);
                        if(settings == null)
                        {
                            settings = SettingsUser.CreateSettings(user.TelegramId,book.id);
                            user.Settings.Add(settings);
                        }
                        if(!user.Books.Any(x => x.id == book.id))
                        {
                            user.Books.Add(book);
                        }
                        
                        await db.SaveChangesAsync();
                        var currentBook = new InlineCallbackCommand(Router.GetValueButton(nameof(Router.BT_SET_CURRENT_BOOK)), InlineCallbackCommands.SetCurrentBook, new EntityCommand(book.id));
                        var option = new OptionMessage();
                        option.MenuInlineKeyboardMarkup = MenuGenerator.InlineKeyboard(1, new List<IInlineContent>() { currentBook });
                        var readerFact = await book.GetBookData();
                        var pathBook = FileWorker.PathBook;
                        option.Message = $"Книга добавлена в базу данных\n";
                        await readerFact.ShowTitle(botClient, update.GetChatId(), option);
                        readerFact.Dispose();
                    }


                }
                else
                {
                    var spl = update.Message.Document.FileName.Split(".");
                    
                    string msg = $"Формат файла должен быть fb2 или pdf, а ваш файл формата {spl[spl.Count() - 1]}";
                    await Common.Message.Send(botClient, update, msg);
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                await Commands.Common.Message.Send(botClient, update.GetChatId(), $"{Router.S_WARNING} Файл поврежден или не валидный, попробуйте другой файл.");
            }
        }

        [InlineCallbackHandler(InlineCallbackCommands.SetCurrentBook)]
        public static async Task SetCurrentBook(ITelegramBotClient botClient,Update update)
        {
            try
            {
                var command = InlineCallbackCommand<EntityCommand>.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if(command != null)
                {
                    using (var db = new AppDbContext())
                    {
                        var book = await db.Books.FirstOrDefaultAsync(x => x.id == command.Data.EntityId);
                        if(book != null)
                        {
                            var user = await db.Users.FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                            if(user != null)
                            {
                                user.CurrentBookId = book.id;
                                await db.SaveChangesAsync();
                                string msg = $"Книга {book.Title} установлена как текущая для чтения";
                                await Common.Message.EditCaption(botClient, update, msg);
                                await MainCommand.MainMenu(botClient, update);
                            }
                        }
                    }

                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
