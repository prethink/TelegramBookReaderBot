using BookBot.Helpers;
using FB2Library;
using FB2Library.Elements;
using FB2Library.Elements.Poem;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using BookBot.Commands;

namespace BookBot.Models.Readers
{
    public class ChapterFb2 : IChapter
    {
        public bool IsSub { get; set; }
        public bool IsSubSub { get; set; }
        public int ChapterId { get; set; }
        public int StartPage { get; set; }
        public int PageCount { get; set; }
        public int FactPageCount { get; set; }
        public int AllPageCount { get; set; }
        public string Title { get; set; }
        public MessageFb2 Content { get; set; }
        public ChapterFb2 Parent { get; set; }
        public List<ChapterFb2> Children { get; set; } = new List<ChapterFb2>();

        public int GetChapterId()
        {
            return ChapterId;
        }

        public string GetName()
        {
            return Title;
        }

        bool IChapter.IsSub()
        {
            return IsSub;
        }

        bool IChapter.IsSubSub()
        {
            return IsSubSub;
        }
    }

    public class MessageFb2
    {
        public List<Fb2Image> Images { get; set; } = new List<Fb2Image>();
        public List<string> ContentBase { get; set; }
        public bool ShowImage { get; set; }
        public string Content { get; set; } = "";
    }

    public class Fb2Image
    {
        public string Name { get; set; }
        public List<byte> Data { get; set; }

        public Stream GetData()
        {
            return new MemoryStream(Data.ToArray());
        }
    }

    public class Fb2Reader : IBookReader
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public List<string> Genres { get; set; } = new List<string>();
        public int PageCount { get; set; }
        public string PathFile { get; set; }
        public FB2File File { get; private set; }
        public List<ChapterFb2> Chapters { get; set; } = new List<ChapterFb2>();

        public const int PAGE_MAX_LENGTH = 900;
        public const int PAGE_MAX_MAX_LENGTH = 1200;
        private bool _disposed;

        public Fb2Reader(string path)
        {
            PathFile = Helpers.FileWorker.BaseDir + path;
        }



        public async Task LoadBookInfo()
        {
            var fileStruct = XDocument.Load(PathFile);
            File = await new FB2Reader().ReadAsync(fileStruct.ToString());
            Title = File?.TitleInfo?.BookTitle?.Text;
            GetGenres();
            GetAuthors();
            GenerateChapters();
        }

        public void GetAuthors()
        {
            foreach (var item in File?.TitleInfo?.BookAuthors)
            {
                var author = $"{item?.FirstName} {item?.MiddleName} {item?.LastName}";
                Authors.Add(author);
            }
        }

        public void GetGenres()
        {
            foreach (var item in File?.TitleInfo?.Genres)
            {
                var genre = item?.Genre?.ToString();
                Genres.Add(genre);
            }
        }

        public int GetProcent(int page)
        {
            float AllCount = PageCount;
            float currentPage = page;
            var result = currentPage / AllCount * 100;
            return (int)result;
        }

        public string GetUnicodePage(int page)
        {
            string pageString = page.ToString();
            pageString = pageString.Replace("0", "0️⃣");
            pageString = pageString.Replace("1", "1️⃣");
            pageString = pageString.Replace("2", "2️⃣");
            pageString = pageString.Replace("3", "3️⃣");
            pageString = pageString.Replace("4", "4️⃣");
            pageString = pageString.Replace("5", "5️⃣");
            pageString = pageString.Replace("6", "6️⃣");
            pageString = pageString.Replace("7", "7️⃣");
            pageString = pageString.Replace("8", "8️⃣");
            pageString = pageString.Replace("9", "9️⃣");
            return pageString;
        }

        public void GenerateChapters()
        {
            int page = 0;
            int chapterId = 1;

            foreach (var item in File.Bodies)
            {
                foreach (var data in item.Sections)
                {
                    GetSections(data, null, ref page, ref chapterId);
                }
            }
        }

        public ChapterFb2 GetSections(SectionItem item, ChapterFb2 parent, ref int page, ref int chapterId)
        {
            var chapter = new ChapterFb2();
            MessageFb2 content = new MessageFb2();
            if (item.Content != null)
            {
                content = GetContent(item.Content.Select(x => x).ToList());
                if (string.IsNullOrWhiteSpace(content.Content) && content.Images.Count == 0 && item.SubSections?.Count == 0)
                {
                    content.Content = "Пустая страница";
                    chapter.Content = content;
                    return chapter;
                }
            }
            chapter.Content = content;
            chapter.Title = item?.Title?.ToString();
            chapter.ChapterId = chapterId;
            chapter.StartPage = page;
            chapter.Content.Content = $"<strong>{chapter.Title}</strong>\n\n" + content.Content;
            chapter.PageCount = chapter.Content.Content.Length / PAGE_MAX_LENGTH;
            chapter.IsSub = parent != null;
            chapter.IsSubSub = parent != null && parent.Parent != null;
            chapter.Parent = parent;
            page += chapter.PageCount;
            chapter.AllPageCount = page;
            Chapters.Add(chapter);
            chapterId++;
            PageCount = page;
            page++;
            if (item?.SubSections?.Count > 0)
            {
                foreach (var children in item.SubSections)
                {
                    var sub = GetSections(children, chapter, ref page, ref chapterId);
                    if (sub != null)
                    {
                        chapter.Children.Add(sub);
                    }
                }
            }
            chapter.FactPageCount = page;
            return chapter;
        }

        public MessageFb2 GetTitleBook()
        {
            var content = new MessageFb2();
            string result = "Книга\n";

            if (!string.IsNullOrWhiteSpace(Title))
            {
                result += $"<strong>{Title}</strong>\n\n";
            }

            if (Authors.Count > 0)
            {
                result += $"<strong>Авторы</strong>\n";
                foreach (var item in Authors)
                {
                    result += $"◽ {item}\n";
                }
            }
            if (Genres.Count > 0)
            {
                result += $"<strong>Жанры</strong>\n";
                foreach (var item in Genres)
                {
                    result += $"◽ {item}\n";
                }
            }
            content.Content = result;
            try
            {
                if (File?.TitleInfo?.Cover?.CoverpageImages?.Count > 0)
                {
                    var imgHref = File.TitleInfo.Cover.CoverpageImages.FirstOrDefault();
                    var imgData = File.Images.FirstOrDefault(x => x.Key.ToLower() == imgHref.HRef.Replace("#", ""));
                    if (!imgData.Equals(default(KeyValuePair<string, BinaryItem>)))
                    {
                        var image = new Fb2Image()
                        {
                            Name = imgData.Key,
                            Data = imgData.Value.BinaryData.ToList()
                        };
                        content.Images.Add(image);
                    }
                }
            }
            catch (Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
            }

            return content;
        }

        public MessageFb2 GetPage(int page)
        {
            foreach (var item in Chapters)
            {
                if (item.StartPage <= page && page <= item.AllPageCount)
                {
                    int needPage = page;

                    if (item.StartPage == 0)
                    {
                        needPage = 0;
                    }
                    else
                    {
                        needPage = Math.Abs(item.AllPageCount - (item.AllPageCount - item.StartPage) - page);
                    }
                    var message = new MessageFb2();
                    bool flg = false;
                    message.Content = item.Content.Content;
                    var sub = message.Content.LastIndexOf(" ", PAGE_MAX_LENGTH * needPage);
                    if (sub != -1)
                    {
                        message.Content = message.Content.Substring(sub);
                    }
                    else
                    {
                        message.Content = message.Content.Substring(PAGE_MAX_LENGTH * needPage);
                    }

                    flg = message.Content[0] == ' ';

                    if (message.Content.Length > PAGE_MAX_LENGTH)
                    {
                        int addIndex = 0;
                        var fixIndex = message.Content.IndexOf(" ", PAGE_MAX_LENGTH);
                        var test = message.Content.Substring(PAGE_MAX_LENGTH);

                        if (fixIndex != -1)
                        {
                            addIndex = fixIndex - PAGE_MAX_LENGTH;
                        }
                        var length = PAGE_MAX_LENGTH + addIndex;
                        if (message.Content.Length > length)
                        {
                            if (flg)
                            {
                                message.Content = message.Content.Substring(0, length);
                            }
                            else
                            {
                                message.Content = message.Content.Substring(0, PAGE_MAX_LENGTH);
                                var index = message.Content.LastIndexOf(" ");
                                message.Content = message.Content.Substring(0, index);
                            }

                        }
                        else
                        {
                            message.Content = message.Content.Substring(0, PAGE_MAX_LENGTH);
                        }


                    }
                    message.Content += $"\n\n<strong>Страница</strong> [ {GetUnicodePage(page + 1)} / {GetUnicodePage(PageCount + 1)} ] - {GetProcent(page)} %";
                    Regex regex = new Regex(@"#\w{0,}.{0,4}");
                    MatchCollection matches = regex.Matches(message.Content);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            message.Images.AddRange(item.Content.Images.Where(x => x.Name.Contains(match.Value.Replace("#", "").Trim())));
                        }
                    }
                    return message;
                }
            }

            return new MessageFb2() { Content = "Страница не найдена :(" };
        }

        public MessageFb2 GetContent(List<IFb2TextItem> data)
        {
            var content = new MessageFb2();
            foreach (var item in data)
            {
                if (item is ImageItem img)
                {
                    var imgData = File.Images.FirstOrDefault(x => x.Key.ToLower() == img.HRef.Replace("#", ""));
                    if (!imgData.Equals(default(KeyValuePair<string, BinaryItem>)))
                    {
                        var image = new Fb2Image()
                        {
                            Name = imgData.Key,
                            Data = imgData.Value.BinaryData.ToList()
                        };
                        content.Images.Add(image);
                        content.Content += img.HRef + "\n\n";
                    }
                }
                else if (item is PoemItem poem)
                {
                    var poemD = GetContent(poem);
                    content.Content += poemD;

                }
                else if (item is SectionItem section)
                {
                    foreach (var itemPoem in section.Content)
                    {
                        if (itemPoem is PoemItem poemGet)
                        {
                            var poemD = GetContent(poemGet);
                            content.Content += poemD;
                        }
                    }
                }
                else
                {
                    if (item is CiteItem cite)
                    {
                        foreach (var itemd in cite?.CiteData)
                        {
                            content.Content += itemd.ToString() + "\n\n";
                        }
                    }
                    else
                    {
                        content.Content += item.ToString() + "\n\n";
                        if (item.ToString().Contains("#"))
                        {
                            var imgData = File.Images.FirstOrDefault(x => x.Key.ToLower() == item.ToString().Replace("#", "").Trim());
                            if (!imgData.Equals(default(KeyValuePair<string, BinaryItem>)))
                            {
                                var image = new Fb2Image()
                                {
                                    Name = imgData.Key,
                                    Data = imgData.Value.BinaryData.ToList()
                                };
                                content.Images.Add(image);
                            }
                        }
                    }
                }
            }

            return content;
        }

        public string GetContent(PoemItem item)
        {
            string content = "";
            foreach (StanzaItem contenPoem in item.Content)
            {
                content += contenPoem.Lines.Select(x => x.ToString()).Aggregate((a, b) => a + "\n" + b) + "\n\n";
            }
            return content;
        }

        public int StartWithChapter(int chapterId)
        {
            int page = 0;
            var chapter = Chapters.FirstOrDefault(x => x.ChapterId == chapterId);
            if (chapter != null)
            {
                page = chapter.StartPage;
            }
            return page;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public string GetTitle()
        {
            return Title;
        }

        public int GetPagesCount()
        {
            return PageCount + 1;
        }


        public async Task<bool> ShowTitle(ITelegramBotClient botClient, long telegramId, OptionMessage option, bool isEdit = false)
        {
            var noImage = FileWorker.PathBook;
            var dataBook = GetTitleBook();
            if(isEdit)
            {
                if (dataBook.Images.Count > 0)
                {
                    var stream = dataBook.Images.FirstOrDefault().GetData();
                    await Commands.Common.Message.EditPhoto(botClient, telegramId, option.MessageId, stream, option);
                    await Commands.Common.Message.EditCaption(botClient, telegramId, option.MessageId, dataBook.Content, option);
                }
                else
                {
                    await Commands.Common.Message.EditPhoto(botClient, telegramId, option.MessageId, noImage, option);
                    await Commands.Common.Message.EditCaption(botClient, telegramId, option.MessageId, dataBook.Content, option);
                }
            }
            else
            {
                if (dataBook.Images.Count > 0)
                {
                    var stream = dataBook.Images.FirstOrDefault().GetData();
                    await Commands.Common.Message.SendPhoto(botClient, telegramId, option.Message + dataBook.Content, stream, option);
                }
                else
                {
                    await Commands.Common.Message.SendPhoto(botClient, telegramId, option.Message + dataBook.Content, noImage, option);
                }
            }

            return true;
        }

        public async Task<bool> ShowContent(ITelegramBotClient botClient, long telegramId, int page, OptionMessage option, bool isEdit = false)
        {
            if(page > 0)
            {
                page--;
            }
            var pageData = GetPage(page);

            if(isEdit)
            {

            }
            else
            {
                if (pageData.Images.Count > 1)
                {
                    await Commands.Common.Message.Send(botClient, telegramId, "Меню для управление книгой", option);
                    var streams = new List<Stream>();
                    await Commands.Common.Message.SendPhotoGroup(botClient, telegramId, pageData.Content, pageData.Images);
                }
                else if (pageData.Images.Count == 1)
                {
                    await Commands.Common.Message.SendPhoto(botClient, telegramId, pageData.Content, pageData.Images.FirstOrDefault().GetData(), option);
                }
                else
                {
                    await Commands.Common.Message.Send(botClient, telegramId, pageData.Content, option);
                }
            }
            return true;

        }

        public int ChaptersCount()
        {
            return Chapters.Count;
        }

        public int GetPageByChapter(int chapterId)
        {
            return StartWithChapter(chapterId);
        }

        public List<IChapter> GetChapters()
        {
            var chapters = new List<IChapter>();
            chapters.AddRange(Chapters);
            return chapters;
        }

        List<string> IBookReader.GetAuthors()
        {
            return Authors;
        }

        List<string> IBookReader.GetGenres()
        {
            return Genres;
        }

        public bool Validation()
        {
            try
            {
                var pageData = GetPage(0);
                return true;
            }
            catch(Exception ex)
            {
                TelegramService.GetInstance().InvokeErrorLog(ex);
                return false;
            }
        }
    }
}
