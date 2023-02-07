using BookBot.Attributes;
using BookBot.Helpers;
using BookBot.Helpers.Extensions;
using BookBot.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static BookBot.Helpers.Extensions.Step;

namespace BookBot.Commands
{
    public class Router
    {
        #region Symbols
        public const string S_WARNING                   = "⚠️";
        public const string S_DELETE_MESSAGE            = "📩";
        public const string S_COINS                     = "💰";
        public const string S_RATING                    = "🏆";
        public const string S_LIKE                      = "💘";
        public const string S_LIKE_MUTUAL               = "💞";
        public const string S_ACTIVITY                  = "🕑";
        public const string S_VIEWED                    = "👁️‍🗨️ ";
        public const string S_NOTIFY                    = "🔔";
        #endregion

        #region RelpyCommands
        public const string BT_START                            = "1b";
        public const string BT_MENU                             = "2b";
        public const string BT_MAIN_MENU                        = "3b";
        public const string BT_SETTINGS                         = "4b";
        public const string BT_READ                             = "5b";
        public const string BT_NEXT_PAGE                        = "6b";
        public const string BT_PREV_PAGE                        = "7b";
        public const string BT_BOOKS                            = "8b";
        public const string BT_ADD_BOOK                         = "9b";
        public const string BT_MYBOOKS                          = "10b";
        public const string BT_SETTINGS_BOOK                    = "11b";
        public const string BT_BACK_BUTTON                      = "12b";
        public const string BT_DELETE_BUTTON                    = "13b";
        public const string BT_DOWNLOAD_BUTTON                  = "14b";

        #endregion

        #region InlineCommand
        public const string BT_SET_CURRENT_BOOK                 = "1i";
        public const string BT_SETTINGS_BOOK_PAGE               = "2i";
        public const string BT_SETTINGS_BOOK_NOTIFY             = "3i";
        public const string BT_SETTINGS_BOOK_CHAPTERS           = "4i";


        public const string BT_GET_USER_SLASH                   = "/usrid";




        #endregion

        delegate Task MessageCommand(ITelegramBotClient botclient, Update update);
        delegate Task CommandInline(Update update,InlineCallbackCommand command);
        private Dictionary<string, MessageCommand> _priorityCommand = new Dictionary<string, MessageCommand>();
        private Dictionary<string, MessageCommand> _commands = new Dictionary<string, MessageCommand>();
        private ITelegramBotClient _botClient;


        private Dictionary<string, MessageCommand> slashCommands;
        private Dictionary<string, MessageCommand> messageCommands;
        private Dictionary<string, MessageCommand> meetCommands;
        private Dictionary<string, MessageCommand> messageCommandsPriority;
        private Dictionary<InlineCallbackCommands, MessageCommand> inlineCommands;

        public Router(ITelegramBotClient botClient)
        {
            _botClient                  = botClient;
            messageCommands             = new Dictionary<string, MessageCommand>();
            meetCommands                = new Dictionary<string, MessageCommand>();
            messageCommandsPriority     = new Dictionary<string, MessageCommand>();
            inlineCommands              = new Dictionary<InlineCallbackCommands, MessageCommand>();
            slashCommands               = new Dictionary<string, MessageCommand>();
            RegisterCommnad();
        }

        public static string GetValueButton(string command)
        {
            var result = ConfigApp.GetSettings<CustomSettings>().GetButton(command);
            return result.Contains("NOT_FOUND") ? command : result;
        }

        public void RegisterCommnad()
        {
            try
            {
                var messageMethods = MethodFinder.FindMessageMenuHandlers();
                var inlineMethods = MethodFinder.FindInlineMenuHandlers();
                var slashCommandMethods = MethodFinder.FindSlashCommandHandlers();

                foreach (var method in messageMethods)
                {
                    bool priority = method.GetCustomAttribute<MessageMenuHandlerAttribute>().Priority;
                    foreach (var command in method.GetCustomAttribute<MessageMenuHandlerAttribute>().Commands)
                    {
                        Delegate serverMessageHandler = Delegate.CreateDelegate(typeof(MessageCommand), method, false);    
                        messageCommands.Add(command, (MessageCommand)serverMessageHandler);
                        if (priority)
                        {
                            messageCommandsPriority.Add(command, (MessageCommand)serverMessageHandler);
                        }
                        

                    }
                }

                foreach (var method in inlineMethods)
                {
                    foreach (var command in method.GetCustomAttribute<InlineCallbackHandlerAttribute>().Commands)
                    {
                        Delegate serverMessageHandler = Delegate.CreateDelegate(typeof(MessageCommand), method, false);
                        inlineCommands.Add(command, (MessageCommand)serverMessageHandler);

                    }
                }

                foreach (var method in slashCommandMethods)
                {
                    foreach (var command in method.GetCustomAttribute<SlashCommandAttribute>().Commands)
                    {
                        Delegate serverMessageHandler = Delegate.CreateDelegate(typeof(MessageCommand), method, false);
                        slashCommands.Add(command, (MessageCommand)serverMessageHandler);

                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public async Task<bool> IsSlashCommand(string command, Update update)
        {
            try
            {
                if (!command.Contains("/"))
                    return false;

                foreach (var commandExecute in slashCommands)
                {
                    if (command.ToLower().Contains(commandExecute.Key.ToLower()))
                    {
                        var requireUpdate = commandExecute.Value.Method.GetCustomAttribute<RequiredTypeUpdateAttribute>();
                        if (requireUpdate != null)
                        {
                            if (!requireUpdate.TypeUpdate.Contains(update.Message.Chat.Type))
                            {
                                await Access.PrivilagesMissing(_botClient, update);
                                return true;
                            }
                        }
                        var privilages = commandExecute.Value.Method.GetCustomAttribute<AccessAttribute>();
                        if (privilages != null && privilages.RequiredPrivilege != null)
                        {
                            //TODO: Check is privilage
                        }
                        else
                        {
                            await commandExecute.Value(_botClient, update);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return false;
            }

        }

        public async Task ExecuteCommandByMessage(string command, Update update)
        {
            try
            {
                if(command.Contains("(") && command.Contains(")"))
                {
                    command = command.Remove(command.LastIndexOf("(") - 1);
                }

                if (await StartHasDeepLink(command, update))
                    return;

                if (await IsSlashCommand(command, update))
                    return;

                if (await IsHaveNextStep(command, update))
                    return;

                foreach (var commandExecute in messageCommands)
                {

                    if (command.ToLower() == commandExecute.Key.ToLower())
                    {
                        var privilages = commandExecute.Value.Method.GetCustomAttribute<AccessAttribute>();
                        var requireDate = commandExecute.Value.Method.GetCustomAttribute<RequireDateAttribute>();
                        var requireUpdate = commandExecute.Value.Method.GetCustomAttribute<RequiredTypeUpdateAttribute>();
                        if(requireUpdate != null)
                        {
                            if(!requireUpdate.TypeUpdate.Contains(update.Message.Chat.Type))
                            {
                                await Access.PrivilagesMissing(_botClient, update);
                                return;
                            }
                        }
                        if (privilages != null && privilages.RequiredPrivilege != null)
                        {
                            if(requireDate != null)
                            {

                            }
                            else
                            {

                            }
                            //TODO: Check is privilage
                        }
                        else
                        {
                            if (requireDate != null)
                            {
                                if (requireDate.TypeData == update.Message.Type)
                                {
                                    await commandExecute.Value(_botClient, update);
                                }
                                else
                                {
                                    await Access.IncorrectTypeData(_botClient, update);
                                }
                            }
                            else
                            {
                                await commandExecute.Value(_botClient, update);
                            }

                        }

                        return;
                    }
                }

                await Access.CommandMissing(_botClient, update);
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public async Task ExecuteCommandByCallBack(Update update)
        {
            try
            {
                var command = InlineCallbackCommand.GetCommandByCallbackOrNull(update.CallbackQuery.Data);
                if (command != null)
                {
                    string msg = $"Пользователь {update.GetInfoUser()} вызвал команду {command.CommandType.GetDescription()}";
                    TelegramService.GetInstance().InvokeCommonLog(msg, TelegramService.TelegramEvents.CommandExecute, ConsoleColor.Magenta);
                    foreach (var commandCallback in inlineCommands)
                    {
                        if (command.CommandType == commandCallback.Key)
                        {
                            var requireUpdate = commandCallback.Value.Method.GetCustomAttribute<RequiredTypeUpdateAttribute>();
                            if (requireUpdate != null)
                            {
                                if (!requireUpdate.TypeUpdate.Contains(update.CallbackQuery.Message.Chat.Type))
                                {
                                    await Access.PrivilagesMissing(_botClient, update);
                                    return;
                                }
                            }
                            var privilages = commandCallback.Value.Method.GetCustomAttribute<AccessAttribute>();
                            if (privilages != null && privilages.RequiredPrivilege != null)
                            {
                                //TODO: Check is privilage
                            }
                            else
                            {
                                await commandCallback.Value(_botClient, update);
                            }
                            return;
                        }
                    }

                    //await Access.CommandMissing(_botClient, update, "CallBack - " + command.CommandType);
                }

            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public async Task<bool> IsHaveNextStep(string command, Update update)
        {
            try
            {
                if (update.HasStep())
                {
                    foreach (var commandExecute in messageCommandsPriority)
                    {
                        if (command.ToLower() == commandExecute.Key.ToLower())
                        {
                            var requireUpdatex = commandExecute.Value.Method.GetCustomAttribute<RequiredTypeUpdateAttribute>();
                            if (requireUpdatex != null)
                            {
                                if (!requireUpdatex.TypeUpdate.Contains(update.Message.Chat.Type))
                                {
                                    await Access.PrivilagesMissing(_botClient, update);
                                    return true;
                                }
                            }
                            await commandExecute.Value(_botClient, update);
                            update.ClearStepUser();
                            return true;
                        }
                    }

                    var cmd = update.GetStepOrNull().CommandDelegate;

                    var privilages = cmd.Method.GetCustomAttribute<AccessAttribute>();
                    var requireDate = cmd.Method.GetCustomAttribute<RequireDateAttribute>();
                    var requireUpdate = cmd.Method.GetCustomAttribute<RequiredTypeUpdateAttribute>();
                    if (requireUpdate != null)
                    {
                        if (!requireUpdate.TypeUpdate.Contains(update.Message.Chat.Type))
                        {
                            await Access.PrivilagesMissing(_botClient, update);
                            return true;
                        }
                    }
                    if (privilages != null && privilages.RequiredPrivilege != null)
                    {
                        if (requireDate != null)
                        {
                            if (requireDate.TypeData == update.Message.Type)
                            {

                            }
                            else
                            {

                            }
                        }
                        //TODO: Check is privilage
                    }
                    else
                    {
                        if (requireDate != null)
                        {
                            if (requireDate.TypeData == update.Message.Type)
                            {
                                await cmd(_botClient, update);
                            }
                            else
                            {
                                await Access.IncorrectTypeData(_botClient, update);
                            }
                        }
                        else
                        {
                            await cmd(_botClient, update);
                        }
                        
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return false;
            }

        }

        public async Task<bool> StartHasDeepLink(string command, Update update)
        {
            try
            {
                if (command.ToLower().Contains("start") && command.Contains(" "))
                {
                    var spl = command.Split(' ');
                    if (!string.IsNullOrEmpty(spl[1]))
                    {
                        if(update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
                        {
                            await RegisterCommand.StartWithArguments(_botClient, update, spl[1]);
                        }
                        return true;
                    }
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return false;
            }
        }
    }
}
