using BookBot.Helpers;
using BookBot.Helpers.Extensions;
using BookBot.Models.Readers;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using static BookBot.TelegramService;

namespace BookBot.Commands.Common
{
    public class Message
    {
        public static async Task<MessageId> CopyMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Message message, long chatId)
        {
            try
            {
                ChatId toMsg = new ChatId(chatId);
                ChatId fromMsg = new ChatId(message.Chat.Id);
                var rMessage = await botClient.CopyMessageAsync(toMsg, fromMsg, message.MessageId);
                return rMessage;
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }
        }
        public static async Task CopyMessage(ITelegramBotClient botClient, List<Telegram.Bot.Types.Message> messages, long chatId)
        {
            try
            {
                ChatId toMsg = new ChatId(chatId);
                foreach (var message in messages)
                {

                    try
                    {
                        ChatId fromMsg = new ChatId(message.Chat.Id);
                        await botClient.CopyMessageAsync(toMsg, fromMsg, message.MessageId);
                    }
                    catch (Exception ex)
                    {
                        GetInstance().InvokeErrorLog(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
            }
        }

        public static async Task<Telegram.Bot.Types.Message> Send(ITelegramBotClient botClient, Update update, string msg, OptionMessage option = null)
        {
            var message = await Send(botClient, update.GetChatId(), msg, option);
            return message;
        }

        public static async Task<Telegram.Bot.Types.Message> Send(ITelegramBotClient botClient, long chatId, string msg, OptionMessage option = null)
        {
  
                Telegram.Bot.Types.Message message;
                if (string.IsNullOrWhiteSpace(msg))
                {
                    return null;
                }

                if (option == null)
                {
                    message = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: msg,
                            parseMode: ParseMode.Html);
                }
                else
                {
                    if (option.ClearMenu)
                    {
                        message = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: msg,
                                parseMode: ParseMode.Html,
                                replyMarkup: new ReplyKeyboardRemove());
                    }
                    else if (option.MenuReplyKeyboardMarkup != null)
                    {
                        message = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: msg,
                                parseMode: ParseMode.Html,
                                replyMarkup: option.MenuReplyKeyboardMarkup);
                    }
                    else if (option.MenuInlineKeyboardMarkup != null)
                    {
                        message = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: msg,
                                parseMode: ParseMode.Html,
                                replyMarkup: option.MenuInlineKeyboardMarkup);
                    }
                    else
                    {
                        message = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: msg,
                                parseMode: ParseMode.Html);
                    }
                }


                GetInstance().InvokeCommonLog($"Бот {GetInstance().BotName} отправил ответ пользователю с id {chatId}\n{msg}", TelegramEvents.Server, ConsoleColor.Yellow);
                return message;
            
        }

        public static async Task<Telegram.Bot.Types.Message[]> SendPhotoGroup(ITelegramBotClient botClient, long chatId, string msg, List<string> filepaths)
        {

            List<InputMediaPhoto> media = new();

            bool isFirst = true;
            int count = 0;
            foreach (var item in filepaths)
            {
                if (isFirst)
                {
                    if(string.IsNullOrWhiteSpace(msg))
                    {
                        media.Add(new InputMediaPhoto(new InputMedia(item)));
                        isFirst = false;
                    }
                    else
                    {
                        media.Add(new InputMediaPhoto(new InputMedia(item)) { Caption = msg, ParseMode = ParseMode.Html });
                        isFirst = false;
                    }

                }
                else
                {
                    media.Add(new InputMediaPhoto(new InputMedia(item)));
                }
                count++;

            }

            var messages = await botClient.SendMediaGroupAsync(chatId, media.ToArray());
            GetInstance().InvokeCommonLog($"Бот {GetInstance().BotName} отправил ответ пользователю с id {chatId}\n{msg}", TelegramEvents.Server, ConsoleColor.Yellow);
            return messages;
        }

        public static async Task<Telegram.Bot.Types.Message[]> SendPhotoGroup(ITelegramBotClient botClient, long chatId, string msg, List<Fb2Image> data)
        {

            List<InputMediaPhoto> media = new();

            bool isFirst = true;
            int count = 0;
            foreach (var item in data)
            {
                if (isFirst)
                {
                    if (string.IsNullOrWhiteSpace(msg))
                    {
                        media.Add(new InputMediaPhoto(new InputMedia(item.GetData(), item.Name)));
                        isFirst = false;
                    }
                    else
                    {
                        media.Add(new InputMediaPhoto(new InputMedia(item.GetData(), item.Name)) { Caption = msg, ParseMode = ParseMode.Html });
                        isFirst = false;
                    }

                }
                else
                {
                    media.Add(new InputMediaPhoto(new InputMedia(item.GetData(), item.Name)));
                }
                count++;

            }

            var messages = await botClient.SendMediaGroupAsync(chatId, media.ToArray());
            GetInstance().InvokeCommonLog($"Бот {GetInstance().BotName} отправил ответ пользователю с id {chatId}\n{msg}", TelegramEvents.Server, ConsoleColor.Yellow);
            return messages;
        }

        public static async Task<Telegram.Bot.Types.Message> SendPhoto(ITelegramBotClient botClient, long chatId, string msg, string filePath, OptionMessage option = null)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return await Send(botClient, chatId, msg, option);
            }

            Telegram.Bot.Types.Message message;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
               return await SendPhoto(botClient, chatId, msg, fileStream, option);
            }

                
            GetInstance().InvokeCommonLog($"Бот {GetInstance().BotName} отправил ответ пользователю с id {chatId}\n{msg}", TelegramEvents.Server, ConsoleColor.Yellow);
        }


        public static async Task<Telegram.Bot.Types.Message> SendPhoto(ITelegramBotClient botClient, long chatId, string msg, Stream stream, OptionMessage option = null)
        {


            Telegram.Bot.Types.Message message;

                if (option == null)
                {
                    message = await botClient.SendPhotoAsync(
                                    chatId: chatId,
                                    photo: new InputOnlineFile(stream),
                                    caption: msg,
                                    parseMode: ParseMode.Html
                                    );
                    return message;
                }
                else
                {
                    if (option.MenuReplyKeyboardMarkup != null)
                    {
                        message = await botClient.SendPhotoAsync(
                                        chatId: chatId,
                                        photo: new InputOnlineFile(stream),
                                        caption: msg,
                                        parseMode: ParseMode.Html,
                                        replyMarkup: option.MenuReplyKeyboardMarkup
                                        );
                        return message;
                    }
                    else if (option.MenuInlineKeyboardMarkup != null)
                    {
                        message = await botClient.SendPhotoAsync(
                                    chatId: chatId,
                                    photo: new InputOnlineFile(stream),
                                    caption: msg,
                                    parseMode: ParseMode.Html,
                                    replyMarkup: option.MenuInlineKeyboardMarkup
                                    );
                        return message;
                    }
                    return null;
                


                GetInstance().InvokeCommonLog($"Бот {GetInstance().BotName} отправил ответ пользователю с id {chatId}\n{msg}", TelegramEvents.Server, ConsoleColor.Yellow);
            }
        }
        public static async Task<Telegram.Bot.Types.Message> SendPhotoWithUrl(ITelegramBotClient botClient, long chatId, string msg, string url, OptionMessage option = null)
        {

                Telegram.Bot.Types.Message message = null;
                if (option == null)
                {
                    message = await botClient.SendPhotoAsync(
                                chatId: chatId,
                                photo: new InputOnlineFile(url),
                                caption: msg,
                                parseMode: ParseMode.Html
                                );
                }
                else
                {
                    if (option.MenuReplyKeyboardMarkup != null)
                    {
                        message = await botClient.SendPhotoAsync(
                                    chatId: chatId,
                                    photo: new InputOnlineFile(url),
                                    caption: msg,
                                    parseMode: ParseMode.Html,
                                    replyMarkup: option.MenuReplyKeyboardMarkup
                                    );
                    }
                    else if (option.MenuInlineKeyboardMarkup != null)
                    {
                        message = await botClient.SendPhotoAsync(
                                    chatId: chatId,
                                    photo: new InputOnlineFile(url),
                                    caption: msg,
                                    parseMode: ParseMode.Html,
                                    replyMarkup: option.MenuInlineKeyboardMarkup
                                    );
                    }
                    else
                    {
                        message = await botClient.SendPhotoAsync(
                                    chatId: chatId,
                                    photo: new InputOnlineFile(url),
                                    caption: msg,
                                    parseMode: ParseMode.Html
                                    );
                    }
                    GetInstance().InvokeCommonLog($"Бот {GetInstance().BotName} отправил ответ пользователю с id {chatId}\n{msg}", TelegramEvents.Server, ConsoleColor.Yellow);
                }
                return message;

            


        }

        public static async Task<Telegram.Bot.Types.Message> SendMediaWithUrl(ITelegramBotClient botClient, long chatId, string msg, string url, OptionMessage option = null)
        {

            Telegram.Bot.Types.Message message = null;
            if (option == null)
            {
                message = await botClient.SendDocumentAsync(
                            chatId: chatId,
                            document: new InputOnlineFile(url),
                            caption: msg,
                            parseMode: ParseMode.Html
                            );
            }
            else
            {
                if (option.MenuReplyKeyboardMarkup != null)
                {
                    message = await botClient.SendDocumentAsync(
                                chatId: chatId,
                                document: new InputOnlineFile(url),
                                caption: msg,
                                parseMode: ParseMode.Html,
                                replyMarkup: option.MenuReplyKeyboardMarkup
                                );
                }
                else if (option.MenuInlineKeyboardMarkup != null)
                {
                    message = await botClient.SendDocumentAsync(
                                chatId: chatId,
                                document: new InputOnlineFile(url),
                                caption: msg,
                                parseMode: ParseMode.Html,
                                replyMarkup: option.MenuInlineKeyboardMarkup
                                );
                }
                else
                {
                    message = await botClient.SendDocumentAsync(
                                chatId: chatId,
                                document: new InputOnlineFile(url),
                                caption: msg,
                                parseMode: ParseMode.Html
                                );
                }
                GetInstance().InvokeCommonLog($"Бот {GetInstance().BotName} отправил ответ пользователю с id {chatId}\n{msg}", TelegramEvents.Server, ConsoleColor.Yellow);
            }
            return message;




        }

        public static async Task SendFile(ITelegramBotClient botClient, long chatId, string msg, string filePath)
        {
 
                if (!System.IO.File.Exists(filePath))
                {
                    await Send(botClient, chatId, msg);
                    return;
                }



                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var file = await botClient.SendDocumentAsync(chatId: chatId,
                                                              document: new InputOnlineFile(fileStream, Path.GetFileName(filePath)),
                                                              caption: msg
                                                              );
                }
            

        }

        public static async Task<Telegram.Bot.Types.Message> Edit(ITelegramBotClient botClient, long chatId, int messageId, string msg, OptionMessage option = null)
        {
            try
            {
                Telegram.Bot.Types.Message message;
                if (string.IsNullOrWhiteSpace(msg))
                {
                    return null;
                }

                if (option == null || option.MenuInlineKeyboardMarkup == null)
                {
                    message = await botClient.EditMessageTextAsync(
                            chatId: chatId,
                            messageId: messageId,
                            text: msg,
                            parseMode: ParseMode.Html);
                }
                else
                {
                    message = await botClient.EditMessageTextAsync(
                            chatId: chatId,
                            messageId: messageId,
                            text: msg,
                            parseMode: ParseMode.Html,
                            replyMarkup: option.MenuInlineKeyboardMarkup);

                }

                return message;
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }

        }

        public static async Task<Telegram.Bot.Types.Message> Edit(ITelegramBotClient botClient, Update update, string msg, OptionMessage option = null)
        {
            try
            {
                long chatId = update.GetChatId();
                int messageId = update.GetMessageId();

                var editmessage = await Edit(botClient, chatId, messageId, msg, option);
                return editmessage;
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }
        }

        public static async Task<Telegram.Bot.Types.Message> EditCaption(ITelegramBotClient botClient, long chatId, int messageId, string msg, OptionMessage option = null)
        {
            try
            {
                Telegram.Bot.Types.Message message;
                if (string.IsNullOrWhiteSpace(msg))
                {
                    return null;
                }

                if (option == null || option.MenuInlineKeyboardMarkup == null)
                {
                    message = await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: msg,
                            parseMode: ParseMode.Html);
                }
                else
                {
                    message = await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption: msg,
                            parseMode: ParseMode.Html,
                            replyMarkup: option.MenuInlineKeyboardMarkup);

                }

                return message;
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }
        }

        public static async Task<Telegram.Bot.Types.Message> EditInline(ITelegramBotClient botClient, long chatId, int messageId, OptionMessage option)
        {
            try
            {
                Telegram.Bot.Types.Message message = null;
                if(option?.MenuInlineKeyboardMarkup != null)
                {
                    message = await botClient.EditMessageReplyMarkupAsync(
                            chatId: chatId,
                            messageId: messageId,
                            replyMarkup: option.MenuInlineKeyboardMarkup);
                }

                return message;
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }
        }

        public static async Task<Telegram.Bot.Types.Message> EditPhoto(ITelegramBotClient botClient, long chatId, int messageId,string photoPath, OptionMessage option = null)
        {
            try
            {
                Telegram.Bot.Types.Message message;
                if (!System.IO.File.Exists(photoPath))
                {
                    return await EditInline(botClient, chatId, messageId, option);
                }


                using (var fileStream = new FileStream(photoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return await EditPhoto(botClient, chatId, messageId, fileStream,option);
                }
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }
        }

        public static async Task<Telegram.Bot.Types.Message> EditPhoto(ITelegramBotClient botClient, long chatId, int messageId, Stream stream, OptionMessage option = null)
        {
            try
            {
                Telegram.Bot.Types.Message message;

                    if (option?.MenuInlineKeyboardMarkup != null)
                    {
                        message = await botClient.EditMessageMediaAsync(
                                chatId: chatId,
                                media: new InputMediaPhoto(new InputMedia(stream, "book")),
                                messageId: messageId,
                                replyMarkup: option.MenuInlineKeyboardMarkup);
                    }
                    else
                    {
                        message = await botClient.EditMessageMediaAsync(
                                chatId: chatId,
                                media: new InputMediaPhoto(new InputMedia(stream, "book")),
                                messageId: messageId);
                    }

                    return message;
                
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }
        }

        public static async Task DeleteChat(ITelegramBotClient botClient, long chatId, int messageId)
        {
            try
            {
                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
            }
        }

        public static async Task<Telegram.Bot.Types.Message> EditWithPhoto(ITelegramBotClient botClient, long chatId, int messageId,string msg, InputMediaBase media, OptionMessage option)
        {
            try
            {
                Telegram.Bot.Types.Message message = null;
                if (option?.MenuInlineKeyboardMarkup != null)
                {
                    message = await botClient.EditMessageMediaAsync(
                            chatId: chatId,
                            messageId: messageId,
                            media: media,
                            replyMarkup: option.MenuInlineKeyboardMarkup);

                    message = await botClient.EditMessageCaptionAsync(
                            chatId: chatId,
                            messageId: messageId,
                            caption:msg,
                            parseMode: ParseMode.Html,
                            replyMarkup: option.MenuInlineKeyboardMarkup);
                }

                return message;
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }
        }

        public static async Task<Telegram.Bot.Types.Message> EditCaption(ITelegramBotClient botClient, Update update, string msg, OptionMessage option = null)
        {
            try
            {
                long chatId = update.GetChatId();
                int messageId = update.GetMessageId();

                var editmessage = await EditCaption(botClient, chatId, messageId, msg, option);
                return editmessage;
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
                return null;
            }
        }

        public static async Task NotifyFromCallBack(ITelegramBotClient botClient, string callbackQueryId, string msg, bool showAlert = true)
        {
            try
            {
                await botClient.AnswerCallbackQueryAsync(callbackQueryId, msg, showAlert);
            }
            catch (Exception ex)
            {
                GetInstance().InvokeErrorLog(ex);
            }
        }
    }
}
