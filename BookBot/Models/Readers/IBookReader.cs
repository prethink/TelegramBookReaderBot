using BookBot.Helpers;
using BookBot.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BookBot.Models.Readers
{
    public interface IBookReader : IDisposable
    {
        public List<string> GetAuthors();
        public List<string> GetGenres();
        public string GetTitle();
        public int GetPagesCount();
        public int ChaptersCount();
        public int GetPageByChapter(int chapterId);
        public List<IChapter> GetChapters();
        public Task<bool> ShowTitle (ITelegramBotClient botClient, long telegramId,OptionMessage option, bool isEdit = false);
        public Task<bool> ShowContent(ITelegramBotClient botClient, long telegramId, int page, OptionMessage option, bool isEdit = false);
        public Task LoadBookInfo();

        public bool Validation();
    }

    public interface IChapter
    {
        public string GetName();
        public int GetChapterId();
        public bool IsSub();
        public bool IsSubSub();
    }

    public class Reader
    {
        public static async Task<IBookReader> LoadBook(string path)
        {
            if(path.Contains(".fb2"))
            {
                var reader = new Fb2Reader(path);
                await reader.LoadBookInfo();
                return reader;
            }
            else if (path.Contains(".pdf"))
            {
                var reader = new PdfReader(path);
                await reader.LoadBookInfo();
                return reader;
            }
            else
            {
                throw new NotImplementedException($"{path} файл пока не поддерживается");
            }
        }
    }

}
