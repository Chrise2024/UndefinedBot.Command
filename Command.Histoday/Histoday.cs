using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using System.Drawing;
using System.Globalization;

namespace Command.Histoday
{
    public class HistodayCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        public HistodayCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("histoday")
                .Description("{0}histoday - 历史上的今天\n使用方法：{0}histoday")
                .ShortDescription("{0}histoday - 历史上的今天")
                .Example("{0}histoday")
                .Action(async (ArgSchematics args) =>
                {
                    string ImageCachePath = GenHistodayImage();
                    await _undefinedApi.Api.SendGroupMsg(
                                    args.GroupId,
                                    _undefinedApi.GetMessageBuilder()
                                        .Image(ImageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                                );
                    SafeDeleteFile(ImageCachePath);
                });
            _undefinedApi.SubmitCommand();
        }
        private void SafeDeleteFile(string tPath)
        {
            try
            {
                if (File.Exists(tPath))
                {
                    File.Delete(tPath);
                }
            }
            catch { }
        }
        private readonly Calendar ZhCNCalendar = new CultureInfo("zh-CN").DateTimeFormat.Calendar;

        private readonly Font s_dateFont = new("Simsun", 60);

        private readonly Font s_contentFont = new("Simsun", 40);

        private readonly SolidBrush s_drawBrush = new(Color.Black);
        public string GenHistodayImage()
        {
            string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"\n{ZhCNCalendar.GetDayOfWeek(DateTime.Now)}";
            string Content = _undefinedApi.Request.Get("https://xiaoapi.cn/API/lssdjt.php").Result;
            Bitmap bg = new(1080, 1500);
            Graphics g = Graphics.FromImage(bg);
            g.Clear(Color.White);
            g.DrawString(CurrentTime, s_dateFont, s_drawBrush, new RectangleF(100, 100, 880, 200));
            g.DrawString(Content, s_contentFont, s_drawBrush, new RectangleF(50, 400, 980, 1100));
            string ImCachePath = Path.Join(_undefinedApi.CachePath, $"{DateTime.Now:HH-mm-ss}.png");
            bg.Save(ImCachePath);
            g.Dispose();
            bg.Dispose();
            return ImCachePath;
        }
    }
}
