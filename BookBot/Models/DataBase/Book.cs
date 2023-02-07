using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookBot.Models.Readers;

namespace BookBot.Models.DataBase
{
    public class Book
    {
        public long id { get; set; }
        public long? Year { get; set; }
        public string Path { get; set; }
        public string Title { get; set; }
        public string Hash { get; set; }
        public List<Author> Authors { get; set; } = new();
        public List<Genre> Genres { get; set; } = new();
        public List<UserBot> Users { get; set; } = new();

        public async Task<IBookReader> GetBookData()
        {
            return await Reader.LoadBook(Path);
        }
    }
}
