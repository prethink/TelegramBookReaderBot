using BookBot.Attributes;
using BookBot.Helpers;
using BookBot.Helpers.Extensions;
using BookBot.Models.DataBase;
using BookBot.Models.DataBase.Base;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BookBot.Commands
{
    public class RegisterCommand
    {
        [MessageMenuHandler(true, nameof(Router.BT_START))]
        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task Start(ITelegramBotClient botClient, Update update)
        {
            await CheckRegister(botClient, update, true);
        }

        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task StartWithArguments(ITelegramBotClient botClient, Update update, string arg)
        {
            try
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    await CheckRegister(botClient, update, true, arg);
                }
                else
                {
                    await CheckRegister(botClient, update, true);
                } 
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }

        }

        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task CheckRegister(ITelegramBotClient botClient, Update update, bool showMsg, string refferId = null)
        {
            try
            {
                if(update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
                {
                    await UserHandler(botClient, update, showMsg, refferId);
                }
                else
                {
                    string msgUser = $"Регистрация группы или другого объекта {update.GetCacheData()}";
                    TelegramService.GetInstance().InvokeCommonLog(msgUser, TelegramService.TelegramEvents.GroupAction, ConsoleColor.White);
                }
            
                
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task UserHandler(ITelegramBotClient botClient, Update update, bool showMsg, string refferId = null)
        {
            try
            {

                bool addRefCoins = false;
                long? existRefUser = null;

                using (var db = new AppDbContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(x => x.TelegramId == update.GetChatId());
                    if (user != null)
                    {
                        if (showMsg)
                        {
                            var result = user.IsActivate;
                            user.IsActivate = true;
                            await db.SaveChangesAsync();
                            if (!result)
                            {
                                await Common.Message.Send(botClient, update, "Бот снова активен :)");
                            }


                            await MainCommand.MainMenu(botClient, update);
                        }
                        return;
                    }

                    var createUser = new UserBot();

                    if (!string.IsNullOrWhiteSpace(refferId) && long.TryParse(refferId, out var idUsr))
                    {
                        var parentUser = db.Users.FirstOrDefault(x => x.TelegramId == idUsr);

                        if (parentUser != null)
                        {
                            createUser.ParentUserId = parentUser.TelegramId;
                            existRefUser = idUsr;
                            addRefCoins = true;
                        }
                    }

                    LinkStatistic? link = null;
                    if (!string.IsNullOrWhiteSpace(refferId))
                    {
                        link = await db.Links.FirstOrDefaultAsync(x => x.Link == refferId);
                        if (link != null)
                        {
                            link.RegCount++;
                        }

                    }

                    var settings = ConfigApp.GetSettings<SettingsConfig>();
                    createUser.TelegramId = update.GetChatId();
                    createUser.RegisteredDate = DateTime.Now;
                    createUser.LastActivity = DateTime.Now;
                    createUser.Login = update.Message.Chat.Username;
                    createUser.FirstName = update.Message.Chat.FirstName;
                    createUser.LastName = update.Message.Chat.LastName;
                    createUser.IsActivate = true;
                    createUser.Link = MessageGenerator.PasswordGenerate(MessageGenerator.PasswordChars.Digits | MessageGenerator.PasswordChars.Alphabet, ConfigApp.GetSettings<SettingsConfig>().WordLength, "u");
                    db.Users.Add(createUser);
                    await db.SaveChangesAsync();
                    await RegisterNewUser(botClient, createUser, existRefUser, link);
                    await MainCommand.MainMenu(botClient, update);
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        [RequiredTypeUpdate(Telegram.Bot.Types.Enums.ChatType.Private)]
        public static async Task RegisterNewUser(ITelegramBotClient botClient, UserBot user, long? existRefUser, Models.DataBase.LinkStatistic link)
        {
            try
            {
                var settings = ConfigApp.GetSettings<SettingsConfig>();
                using (var db = new AppDbContext())
                {
                    if(settings.ShowNotifyRegisterUserForAdmin)
                    {
                        var photos = await botClient.GetUserProfilePhotosAsync(user.TelegramId);
                        long allCountUser = db.Users.Count();
                        
                        foreach (var telegramId in settings.Admins)
                        {
                            string msg = MessagesPattern.GetMessage(nameof(MessagesPattern.MSG_NEW_USER));
                            msg += "\n🆔 " + user.TelegramId;
                            msg += "\n🙆‍♂️ " + user.GetName();
                            msg += "\n\n\nВсего пользователей: " + allCountUser;
                            if(link != null)
                            {
                                msg += $"\nРегистрация с {link.Description} - количество регистраций {link.RegCount}";
                            }
                            if(existRefUser != null)
                            {
                                msg += $"\nПривел человека {existRefUser.Value}";
                            }
                            if (photos.TotalCount > 0)
                            {
                                string photoId = photos.Photos[0][1].FileId;
                                await Common.Message.SendPhotoWithUrl(botClient, telegramId, msg, photoId);
                            }
                            else
                            {
                                await Common.Message.Send(botClient, telegramId, msg);
                            }
                        }
                    }
                    TelegramService.GetInstance().InvokeCommonLog($"В боте зарегистрирован новый пользователь! Id:{user.TelegramId} Имя:{user.GetName()}", TelegramService.TelegramEvents.Register, ConsoleColor.Green);
                }
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }
    }
}
