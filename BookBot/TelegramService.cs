using System.ComponentModel;
using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace BookBot
{
    public class TelegramService
    {
        public enum TelegramEvents
        {
            [Description("Initialization")]
            Initialization,
            [Description("Register")]
            Register,
            [Description("Message")]
            Message,
            [Description("Server")]
            Server,
            [Description("BlockedBot")]
            BlockedBot,
            [Description("CountyAPI")]
            CountyAPI,
            [Description("CommandExecute")]
            CommandExecute,
            [Description("NextUser")]
            NextUser,
            [Description("GroupAction")]
            GroupAction,
            [Description("PhotoBattle")]
            PhotoBattle,
        }

        public string BotName { get; private set; }
        public ITelegramBotClient botClient { get; private set; }
        private Handler _handler;
        private CancellationTokenSource _cts;
        private ReceiverOptions _options;
        public delegate void ErrorEvent(Exception ex, long? id);
        public delegate void CommonEvent(string msg, TelegramEvents typeEvent, ConsoleColor color);
        public event ErrorEvent OnLogError;
        public event CommonEvent OnLogCommon;

        public static TelegramService Instance { get; private set; }

        public string Token { get; private set; }
        public bool IsWork { get; private set; }
        private TelegramService(string token)
        {
            Token = token;
        }

        public static TelegramService GetInstance(string token = "")
        {
            if (Instance == null)
            {
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception($"Пустой {nameof(token)} при создание объекта {typeof(TelegramService)}");
                }
                Instance = new TelegramService(token);
            }

            return Instance;
        }


        public async Task Start()
        {
            try
            {
                botClient = new TelegramBotClient(Token);
                _handler = new Handler(botClient);
                _cts = new CancellationTokenSource();
                _options = new ReceiverOptions { AllowedUpdates = { } };

                await ClearUpdates();

                botClient.StartReceiving(
                            _handler.HandleUpdateAsync,
                            _handler.HandleErrorAsync,
                            _options,
                            cancellationToken: _cts.Token);


                var client = await botClient.GetMeAsync();
                BotName = client.Username;
                GetInstance().InvokeCommonLog($"Бот {client.Username} запущен.", TelegramEvents.Initialization, ConsoleColor.Yellow);

                IsWork = true;
            }
            catch (Exception ex)
            {
                IsWork = false;
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public async Task Stop()
        {
            try
            {
                _cts.Cancel();

                await Task.Delay(3000);
                IsWork = false;
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }
        }

        public async Task ChangeToken(string token)
        {
            await Stop();
            Token = token;
            await Start();
        }

        /// <summary>
        /// Очистка очереди команд перед запуском
        /// </summary>
        public async Task ClearUpdates()
        {
            try
            {
                var update = await botClient.GetUpdatesAsync();
                foreach (var item in update)
                {
                    var offset = item.Id + 1;
                    await botClient.GetUpdatesAsync(offset);
                }
            }
            catch(Exception ex)
            {
                InvokeErrorLog(ex);
            }

        }

        public void InvokeCommonLog(string msg, TelegramEvents typeEvent = TelegramEvents.Message, ConsoleColor color = ConsoleColor.Blue)
        {
            OnLogCommon?.Invoke(msg, typeEvent, color);
        }

        public void InvokeErrorLog(Exception ex, long? id = null)
        {
            OnLogError?.Invoke(ex,id);
        }


    }
}