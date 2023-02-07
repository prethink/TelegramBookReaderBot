using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Diagnostics;
using Telegram.Bot.Exceptions;
using System.Reflection;
using BookBot.Commands;
using BookBot.Models.DataBase.Base;
using BookBot.Helpers.Extensions;

namespace BookBot
{
    public class Handler
    {
        private ITelegramBotClient _botClient;
        private Router _router;
        public Handler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
            _router = new Router(_botClient);
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Unknown:
                        break;
                    case UpdateType.Message:
                        await HandleMessageRoute(botClient, update, cancellationToken);
                        break;
                    case UpdateType.InlineQuery:
                        await HandleInlineRoute(botClient, update, cancellationToken);
                        break;
                    case UpdateType.ChosenInlineResult:
                        break;
                    case UpdateType.CallbackQuery:
                        await HandleCallbackQuery(botClient, update, cancellationToken);
                        break;
                    case UpdateType.EditedMessage:
                        break;
                    case UpdateType.ChannelPost:
                        break;
                    case UpdateType.EditedChannelPost:
                        break;
                    case UpdateType.ShippingQuery:
                        break;
                    case UpdateType.PreCheckoutQuery:
                        break;
                    case UpdateType.Poll:
                        break;
                    case UpdateType.PollAnswer:
                        break;
                    case UpdateType.MyChatMember:
                        break;
                    case UpdateType.ChatMember:
                        break;
                    case UpdateType.ChatJoinRequest:
                        break;
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        private async Task HandleCallbackQuery(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await _router.ExecuteCommandByCallBack(update);
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        private async Task HandleInlineRoute(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };
                //TODO Logging exception
                //return Task.CompletedTask;
            }
            catch(Exception ex)
            {
                //TODO Logging exception
            }

        }

        async Task HandleMessageRoute(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Message.Type)
                {
                    case MessageType.Unknown:
                        break;
                    case MessageType.Text:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Photo:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Audio:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Video:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Voice:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Document:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Sticker:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Location:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Contact:
                        await HandleMessageContact(botClient, update, cancellationToken);
                        break;
                    case MessageType.Venue:
                        break;
                    case MessageType.Game:
                        break;
                    case MessageType.VideoNote:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Invoice:
                        break;
                    case MessageType.SuccessfulPayment:
                        break;
                    case MessageType.WebsiteConnected:
                        break;
                    case MessageType.ChatMembersAdded:
                        break;
                    case MessageType.ChatMemberLeft:
                        break;
                    case MessageType.ChatTitleChanged:
                        break;
                    case MessageType.ChatPhotoChanged:
                        break;
                    case MessageType.MessagePinned:
                        break;
                    case MessageType.ChatPhotoDeleted:
                        break;
                    case MessageType.GroupCreated:
                        break;
                    case MessageType.SupergroupCreated:
                        break;
                    case MessageType.ChannelCreated:
                        break;
                    case MessageType.MigratedToSupergroup:
                        break;
                    case MessageType.MigratedFromGroup:
                        break;
                    case MessageType.Poll:
                        await HandleMessageText(botClient, update, cancellationToken);
                        break;
                    case MessageType.Dice:
                        break;
                    case MessageType.MessageAutoDeleteTimerChanged:
                        break;
                    case MessageType.ProximityAlertTriggered:
                        break;
                    case MessageType.WebAppData:
                        break;
                    case MessageType.VideoChatScheduled:
                        break;
                    case MessageType.VideoChatStarted:
                        break;
                    case MessageType.VideoChatEnded:
                        break;
                    case MessageType.VideoChatParticipantsInvited:
                        break;
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }

        }

        async Task HandleMessageText(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                string command = update.Message.Text ?? update.Message.Type.ToString();
                using(var db = new AppDbContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.TelegramId == update.GetChatId());
                    if(user != null && user.IsBan)
                    {
                        TelegramService.GetInstance().InvokeCommonLog($"Пользователь: {update.GetInfoUser()} забанен, команда не будет обработана");
                        return;
                    }
                }

                TelegramService.GetInstance().InvokeCommonLog($"Пользователь :{update.GetInfoUser()} написал {command}");


                await _router.ExecuteCommandByMessage(command, update);
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }

        }

        async Task HandleMessageContact(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                long chatId = update.GetChatId();
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }
    }


}
