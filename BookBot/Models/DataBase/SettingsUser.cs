using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Models.DataBase
{
    [Table("settings_user")]
    public class SettingsUser
    {
        [NotMapped]
        public const long MIN_REPEAT_TIME = 5;
        [NotMapped]
        public const long MAX_REPEAT_TIME = 1440;
        
        public long id { get; set; }
        [Column("user_id")]
        public long UserId { get; set; }
        public UserBot User { get; set; }
        [Column("book_id")]
        public long BookId { get; set; }
        public Book Book { get; set; }
        [Column("repeat_time")]
        public TimeSpan RepeatTime { get; set; }
        [Column("next_notify_time")]
        public DateTime NextNotifyTime { get; set; }
        [Column("current_page")]
        public int CurrentPage { get; set; }
        [Column("is_repeat")]
        public bool IsRepeat { get; set; }

        public static SettingsUser CreateSettings(long userId, long bookId)
        {
            return CreateSettings(userId, bookId, new TimeSpan(1, 0, 0));
        }

        public static SettingsUser CreateSettings(long userId, long bookId, TimeSpan repeatTime)
        {
            var settings = new SettingsUser();
            settings.UserId = userId;
            settings.BookId = bookId;
            settings.RepeatTime = repeatTime;
            settings.NextNotifyTime = DateTime.Now.Add(settings.RepeatTime);
            settings.IsRepeat = true;
            return settings;
        }
    }
}
