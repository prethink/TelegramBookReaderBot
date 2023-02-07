using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace BookBot.Helpers.Extensions
{
    public static class Step
    {
        static Dictionary<long, CommandStep> _step = new();

        public static void RegisterNextStep(this Update update, CommandStep command)
        {
            long userId = update.GetChatId();
            ClearStepUser(update);
            _step.Add(userId, command);
        }

        public static CommandStep GetStepOrNull(this Update update)
        {
            long userId = update.GetChatId();
            return _step.FirstOrDefault(x => x.Key == userId).Value;
        }

        public static void ClearStepUser(this Update update)
        {
            long userId = update.GetChatId();
            if (HasStep(update))
            {
                _step.Remove(userId);
            }

        }

        public static bool HasStep(this Update update)
        {
            long userId = update.GetChatId();
            if(_step.ContainsKey(userId))
            {
                var data = update.GetStepOrNull();
                if(data.ExpiriedTime != null)
                {
                    if(DateTime.Now > data.ExpiriedTime)
                    {
                        data.ExpiriedTime = null;
                        _step.Remove(userId);
                        return false;
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class CommandStep
    {
        public delegate Task Command(ITelegramBotClient botClient, Update update);

        public Command CommandDelegate { get; set; }
        public DateTime? ExpiriedTime { get; set; }

        public static CommandStep GetCommand(Command command)
        {
            return new CommandStep() { CommandDelegate = command };
        }

        public static CommandStep GetCommand(Command command, DateTime expiriedTime)
        {
            return new CommandStep() { CommandDelegate = command, ExpiriedTime = expiriedTime};
        }
    }
}
