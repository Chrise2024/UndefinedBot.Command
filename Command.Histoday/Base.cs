using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using System.Drawing;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Command.Histoday
{
    public class Base
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        public Base(string pluginName)
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
            _undefinedApi.RegisterCommand("moyu")
                .Alias(["摸鱼","摸鱼日记"])
                .Description("{0}moyu - 摸鱼日记\n使用方法：{0}moyu")
                .ShortDescription("{0}moyu - 摸鱼日记")
                .Example("{0}moyu")
                .Action(async (ArgSchematics args) =>
                {
                    try
                    {
                        JObject Resp = JObject.Parse(await _undefinedApi.Request.Get("https://api.vvhan.com/api/moyu?type=json"));
                        string? ImUrl = Resp.Value<string>("url");
                        if (ImUrl != null)
                        {
                            await _undefinedApi.Api.SendGroupMsg(
                                            args.GroupId,
                                            _undefinedApi.GetMessageBuilder()
                                                .Image(ImUrl, ImageSendType.Url, ImageSubType.Normal).Build()
                                        );
                        }
                    }
                    catch { }
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
