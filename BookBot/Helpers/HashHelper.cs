using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace BookBot.Helpers
{
    internal class HashHelper
    {
        public static string HashFile(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(Helpers.FileWorker.BaseDir + path))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
