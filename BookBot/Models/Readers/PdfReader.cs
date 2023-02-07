using BookBot.Helpers;
using Docnet.Core;
using Docnet.Core.Converters;
using Docnet.Core.Models;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace BookBot.Models.Readers
{
    public class PdfReader : IBookReader
    {
        public string PathFile { get; set; }
        readonly private PageDimensions pageSize = new PageDimensions(1080, 1920);
        private bool _disposed;

        public PdfReader(string path)
        {
            PathFile = Helpers.FileWorker.BaseDir + path;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public int GetPagesCount()
        {
            using (var library = DocLib.Instance)
            {
                using (var docReader = library.GetDocReader(PathFile, pageSize))
                {
                    return docReader.GetPageCount();
                }
            }
        }

        public string GetTitle()
        {
            string result = "Книга\n";
            try
            {
                using (var library = DocLib.Instance)
                {
                    using (var docReader = library.GetDocReader(PathFile, pageSize))
                    {
                        using (var pageReader = docReader.GetPageReader(0))
                        {
                            var rawBytes = pageReader.GetImage(new NaiveTransparencyRemover(255, 255, 255));

                            var width = pageReader.GetPageWidth();
                            var height = pageReader.GetPageHeight();


                            using var bmp = new Bitmap(width, height);

                            AddBytes(bmp, rawBytes);

                            using var stream = new MemoryStream();

                            bmp.Save(stream, ImageFormat.Png);
                            var file = FileWorker.SaveImage(-1, stream, "TempImg.png");
         
          
                            string textResult = "";


  
                            var ocrengine = new TesseractEngine(FileWorker.BaseDir +"/tessdata", "rus", EngineMode.Default);
                            var img = Pix.LoadFromFile(file);
                            var res = ocrengine.Process(img);
                            textResult = res.GetText();

                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                            if (!string.IsNullOrWhiteSpace(textResult))
                            {
                                result += $"<strong>{textResult}</strong>\n\n";
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return result;
            }



        }


        static void AddBytes(Bitmap bmp, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var pNative = bmpData.Scan0;

            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bmp.UnlockBits(bmpData);
        }

        static void DrawRectangles(Bitmap bmp, IEnumerable<Character> characters)
        {
            var pen = new Pen(Color.Red);

            using var graphics = Graphics.FromImage(bmp);

            foreach (var c in characters)
            {
                var rect = new Rectangle(c.Box.Left, c.Box.Top, c.Box.Right - c.Box.Left, c.Box.Bottom - c.Box.Top);
                graphics.DrawRectangle(pen, rect);
            }
        }

        public async Task<bool> ShowTitle(ITelegramBotClient botClient, long telegramId, OptionMessage option, bool isEdit = false)
        {
            string textResult = GetTitle();
            try
            {
                using (var library = DocLib.Instance)
                {
                    using (var docReader = library.GetDocReader(PathFile, pageSize))
                    {
                        using (var pageReader = docReader.GetPageReader(0))
                        {
                            var rawBytes = pageReader.GetImage(new NaiveTransparencyRemover(255, 255, 255));

                            var width = pageReader.GetPageWidth();
                            var height = pageReader.GetPageHeight();


                            using var bmp = new Bitmap(width, height);

                            AddBytes(bmp, rawBytes);

                            using var stream = new MemoryStream();

                            bmp.Save(stream, ImageFormat.Png);
                            var file = FileWorker.SaveImage(telegramId, stream, "TempImg.png");


                            if (isEdit)
                            {
                                await Commands.Common.Message.EditPhoto(botClient, telegramId, option.MessageId, file, option);
                                //if (!string.IsNullOrEmpty(textResult))
                                //{
                                //    await Commands.Common.Message.EditCaption(botClient, telegramId, option.MessageId, textResult, option);
                                //}

                            }
                            else
                            {
                                await Commands.Common.Message.SendPhoto(botClient, telegramId, option.Message , file, option);
                            }

                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                var noImage = FileWorker.NoFoundPage;

                if (isEdit)
                {
                    await Commands.Common.Message.EditPhoto(botClient, telegramId, option.MessageId, noImage, option);

                }
                else
                {
                    await Commands.Common.Message.SendPhoto(botClient, telegramId, option.Message, noImage, option);
                }
                return false;
            }

        }

        public async Task<bool> ShowContent(ITelegramBotClient botClient, long telegramId, int page, OptionMessage option, bool isEdit = false)
        {
            if (page > 0)
            {
                page--;
            }
            try
            {
                using (var library = DocLib.Instance)
                {
                    using (var docReader = library.GetDocReader(PathFile, pageSize))
                    {
                        using (var pageReader = docReader.GetPageReader(page))
                        {
                            var rawBytes = pageReader.GetImage(new NaiveTransparencyRemover(255, 255, 255));

                            var width = pageReader.GetPageWidth();
                            var height = pageReader.GetPageHeight();

                            var characters = pageReader.GetCharacters();

                            using var bmp = new Bitmap(width, height);

                            AddBytes(bmp, rawBytes);

                            using var stream = new MemoryStream();

                            bmp.Save(stream, ImageFormat.Png);
                            var file = FileWorker.SaveImage(telegramId, stream, "TempImg.png");

                            if (isEdit)
                            {
                                await Commands.Common.Message.EditPhoto(botClient, telegramId, option.MessageId, file, option);

                            }
                            else
                            {
                                await Commands.Common.Message.SendPhoto(botClient, telegramId, option.Message, file, option);
                            }

                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                var noImage = FileWorker.NoFoundPage;
                if (isEdit)
                {
                    await Commands.Common.Message.EditPhoto(botClient, telegramId, option.MessageId, noImage, option);

                }
                else
                {
                    await Commands.Common.Message.SendPhoto(botClient, telegramId, option.Message, noImage, option);
                }
            }

            return true;

        }

        public int ChaptersCount()
        {
            return 0;
        }

        public int GetPageByChapter(int chapterId)
        {
            return 0;
        }

        public List<IChapter> GetChapters()
        {
            return new List<IChapter>();
        }

        public List<string> GetAuthors()
        {
            return new List<string>();
        }

        public List<string> GetGenres()
        {
            return new List<string>();
        }

        public async Task LoadBookInfo()
        {

        }

        public bool Validation()
        {
            try
            {
                using (var library = DocLib.Instance)
                {
                    using (var docReader = library.GetDocReader(PathFile, pageSize))
                    {
                        using (var pageReader = docReader.GetPageReader(0))
                        {
                            var rawBytes = pageReader.GetImage(new NaiveTransparencyRemover(255, 255, 255));

                            var width = pageReader.GetPageWidth();
                            var height = pageReader.GetPageHeight();


                            using var bmp = new Bitmap(width, height);

                            AddBytes(bmp, rawBytes);

                            using var stream = new MemoryStream();

                            bmp.Save(stream, ImageFormat.Png);
                            var file = FileWorker.SaveImage(-1, stream, "TempImg.png");


                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return false;
            }
        }
    }
}
