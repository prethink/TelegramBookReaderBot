using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BookBot.Helpers
{
    public static class FileWorker
    {
        public static string BaseDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string PathBook = BaseDir + "/Uploads/Images/book.png";
        public static string NoFoundPage = BaseDir + "/Uploads/Images/404.jpg";


        public static async Task<string> DownloadFile(ITelegramBotClient botClient, long telegramId, string fileId, string fileName)
        {
            try
            {
                string folder = $"/Uploads/Users/{telegramId}/";
                string fullPath = BaseDir + folder + "/" +fileName;
                string dbpath = folder + "/" +fileName;
                if (!Directory.Exists(BaseDir + folder))
                {
                    Directory.CreateDirectory(BaseDir + folder);
                }
                await using Stream fileStream = System.IO.File.OpenWrite(fullPath);
                var file = await botClient.GetInfoAndDownloadFileAsync(
                    fileId: fileId,
                    destination: fileStream);
                return dbpath;
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return "";
            }
        }



        public static string SaveImage(long telegramId,MemoryStream stream, string fileName)
        {
            try
            {
                string folder = $"/Uploads/Users/{telegramId}/";
                string fullPath = BaseDir + folder + "/" + fileName;
                string dbpath = folder + "/" + fileName;
                if (!Directory.Exists(BaseDir + folder))
                {
                    Directory.CreateDirectory(BaseDir + folder);
                }
                File.WriteAllBytes(fullPath, stream.ToArray());
                return fullPath;
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return "";
            }
        }

    }
}
