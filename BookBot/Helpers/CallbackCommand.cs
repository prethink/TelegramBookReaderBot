using BookBot.Models;
using BookBot.Models.CallbackCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BookBot.Helpers
{
    public class InlineCallbackCommand<T> : InlineCallbackCommand where T : CallbackBaseCommand
    {
        [JsonProperty("d")]
        public T Data { get; set; }
        [JsonConstructor]
        public InlineCallbackCommand(string buttonName, InlineCallbackCommands commandType, T data) : base(buttonName, commandType, data)
        {
            ButtonName = buttonName;
            CommandType = commandType;
            Data = data;
        }

        public static InlineCallbackCommand<T> GetCommandByCallbackOrNull(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<InlineCallbackCommand<T>>(data);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetTextButton()
        {
            return ButtonName;
        }

        public object GetContent()
        {
            var result = JsonConvert.SerializeObject(this);
            var byteSize = result.Length * sizeof(Char);
            if (byteSize > MAX_SIZE_CALLBACK_DATA)
            {
                throw new Exception($"Превышен лимит для callback_data {byteSize} > {MAX_SIZE_CALLBACK_DATA}. Попробуйте уменьшить количество данных в команде");
            }
            return result;
        }
    }

    public class InlineCallbackCommand : IInlineContent
    {
        public const int MAX_SIZE_CALLBACK_DATA = 128;

        [JsonIgnore]
        public string ButtonName { get; set; }
        [JsonProperty("c")]
        public InlineCallbackCommands CommandType { get; set; }
        [JsonProperty("d")]
        public CallbackBaseCommand Data { get; set; }
        [JsonConstructor]
        public InlineCallbackCommand(string buttonName, InlineCallbackCommands commandType, CallbackBaseCommand data)
        {
            ButtonName = buttonName;
            CommandType = commandType;
            Data = data;
        }

        public InlineCallbackCommand(string buttonName, InlineCallbackCommands commandType)
        {
            ButtonName = buttonName;
            CommandType = commandType;
            Data = new CallbackBaseCommand();
        }

        public static InlineCallbackCommand GetCommandByCallbackOrNull(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<InlineCallbackCommand>(data);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetTextButton()
        {
            return ButtonName;
        }

        public object GetContent()
        {
            var result = JsonConvert.SerializeObject(this);
            var byteSize = result.Length * sizeof(Char);
            if (byteSize > MAX_SIZE_CALLBACK_DATA)
            {
                throw new Exception($"Превышен лимит для callback_data {byteSize} > {MAX_SIZE_CALLBACK_DATA}. Попробуйте уменьшить количество данных в команде");
            }
            return result;
        }
    }

    public class InlineURL : IInlineContent
    {
        public string ButtonName { get; set; }
        public string URL { get; set; }

        public InlineURL(string buttonName, string url)
        {
            ButtonName = buttonName;
            URL = url;
        }

        public object GetContent()
        {
            return URL;
        }

        public string GetTextButton()
        {
            return ButtonName;
        }
    }

    public class InlineWebApp : IInlineContent
    {
        public string ButtonName { get; set; }
        public string WebAppUrl { get; set; }

        public InlineWebApp(string buttonName, string webAppUrl)
        {
            ButtonName = buttonName;
            WebAppUrl = webAppUrl;
        }

        public object GetContent()
        {
            var webApp = new WebAppInfo().Url = WebAppUrl;
            return webApp;
        }

        public string GetTextButton()
        {
            return ButtonName;
        }
    }

    public interface IInlineContent
    {
        public string GetTextButton();
        public object GetContent();

    }
}
