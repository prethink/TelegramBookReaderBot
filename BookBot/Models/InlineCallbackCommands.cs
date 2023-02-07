using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Models
{
    public enum InlineCallbackCommands : ushort
    {
        [Description("None")]
        None = 594,
        [Description("SetCurrentBook")]
        SetCurrentBook,
        BookChangeChapter,
        BookChangePage,
        BookNotify,
        MyBookNext,
        MyBookPrev,
        MyBookRead,
        BookNotifyChangeState,
        BookNotifyBack,
        MyBookDelete,
        BookSelectChapter,
        BookChangeChapterHandler,
        BookChangeRepeatTime,
        MyBookDownload,
    }
}
