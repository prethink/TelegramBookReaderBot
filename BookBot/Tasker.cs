using BookBot.Commands;
using BookBot.Helpers;
using BookBot.Models.DataBase.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BookBot.TelegramService;

namespace BookBot
{
    public class Tasker
    {
        public int TimeOut { get; set; }

        public Tasker(int timeOut)
        {
            TimeOut = timeOut;
        }

        public async Task Start()
        {
            while(true)
            {
                await NotifyBookUser();
                await Task.Delay(TimeOut * 1000);
           }
        }

        public static async Task NotifyBookUser()
        {
            try
            {
                using(var db = new AppDbContext())
                {
                    var currentTask = await db.Users.Include(x => x.Settings).Where(x => x.IsActivate && x.CurrentBookId != null && x.Settings.Any(x => x.IsRepeat)).ToListAsync();
                    var botClient = TelegramService.GetInstance().botClient;
                    foreach (var user in currentTask)
                    {
                        var task = user.Settings.FirstOrDefault(x => x.BookId == user.CurrentBookId.Value);
                        if(task != null && task.IsRepeat)
                        {
                            if(DateTime.Now > task.NextNotifyTime)
                            {
                                try
                                {
                                    await Commands.BookCommand.ReadBook(botClient, user.TelegramId, false);
                                }
                                catch(Exception ex)
                                {
                                    if(ex.Message.Contains("Forbidden: bot was blocked by the user"))
                                    {
                                        user.IsActivate = false;
                                        await db.SaveChangesAsync();
                                        TelegramService.GetInstance().InvokeCommonLog($"Отключаю запись пользователя {user.GetName()}",TelegramEvents.BlockedBot, ConsoleColor.White);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
