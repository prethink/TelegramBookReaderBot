using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Models.DataBase
{
    [Table("users")]
    public class UserBot
    {
        [Key]
        [Column("telegram_id")]
        public long TelegramId { get; set; }
        [Column("parent_user_id")]
        public long? ParentUserId { get; set; }
        public UserBot? ParentUser { get; set; }
        [Column("registered_date")]
        public DateTime RegisteredDate { get; set; }
        [Column("last_activity")]
        public DateTime LastActivity { get; set; }
        [Column("login")]
        public string? Login { get; set; }
        [Column("firstname")]
        public string? FirstName { get; set; }
        [Column("lastname")]
        public string? LastName { get; set; }
        [Column("is_ban")]
        public bool IsBan { get; set; }
        [Column("is_active")]
        public bool IsActivate { get; set; }
        [Column("activity")]
        public long Activity { get; set; }
        [Column("link")]
        public string Link { get; set; }
        [Column("current_book_id")]
        public long? CurrentBookId { get; set; }
        public Book? CurrentBook { get; set; }
        public List<Book> Books { get; set; } = new();
        public List<SettingsUser> Settings { get; set; } = new();



        public string GetName()
        {
            if (!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName))
            {
                string tempName = "";
                tempName += FirstName + " ";
                tempName += LastName;
                return tempName;
            }
            else if (!string.IsNullOrEmpty(Login))
            {
                return Login;
            }
            return "Имя не определено";
        }

        public void AddActivity(long activity)
        {
            this.Activity += activity;
            this.LastActivity = DateTime.Now;
        }
    }
}
