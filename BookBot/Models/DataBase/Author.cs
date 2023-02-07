using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Models.DataBase
{
    public class Author
    {
        public long id { get; set; }
        public string Name { get; set; }
        public List<Book> Books { get; set; } = new();
    }
}
