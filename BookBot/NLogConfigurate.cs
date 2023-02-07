using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookBot
{
    public static class NLogConfigurate
    {
        public static void Configurate()
        {
            string basedir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configuration = new NLog.Config.LoggingConfiguration();
            Console.WriteLine(basedir);
            var logsType = Enum.GetValues(typeof(TelegramService.TelegramEvents));

            foreach (var logType in logsType)
            {
                var type = logType.ToString();
                var logfile = new NLog.Targets.FileTarget(type) { FileName = basedir + "/logs/" + type + "/log-${date:format=\\dd.\\MM.\\yyyy}.txt" };
                configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile, type);
            }

            string errorType = "Error";
            var logError = new NLog.Targets.FileTarget(errorType) { FileName = basedir + "/logs/" + errorType + "/log-${date:format=\\dd.\\MM.\\yyyy}.txt" };
            configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logError, errorType);

            NLog.LogManager.Configuration = configuration;
        }
    }
}
