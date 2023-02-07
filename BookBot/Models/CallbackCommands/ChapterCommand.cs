using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Models.CallbackCommands
{
    internal class ChapterCommand : CallbackBaseCommand
    {
        [JsonIgnore]
        public const long PAGE_SIZE = 5;

        [JsonProperty("1")]
        public long BookId { get; set; }
        [JsonProperty("2")]
        public long Page { get; set; }
        [JsonProperty("3")]
        public long PageSize { get; set; }
        [JsonProperty("4")]
        public long SelectedChapter { get; set; }
        public ChapterCommand(long bookId, long page, long pageSise, long selectedChapter)
        {
            BookId = bookId;
            Page = page;
            PageSize = pageSise;
            SelectedChapter = selectedChapter;
        }
    }
}
