using System.Drawing;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core;
using UndefinedBot.Core.Utils;

namespace Command.Information
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
                .Description("历史上的今天")
                .ShortDescription("历史上的今天")
                .Usage("{0}histoday")
                .Example("{0}histoday")
                .Action(async (args) =>
                {
                    string imageCachePath = GenHistodayImage();
                    await _undefinedApi.Api.SendGroupMsg(
                                    args.GroupId,
                                    _undefinedApi.GetMessageBuilder()
                                        .Image(imageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                                );
                    SafeDeleteFile(imageCachePath);
                });
            _undefinedApi.RegisterCommand("moyu")
                .Alias(["摸鱼","摸鱼日记"])
                .Description("摸鱼日记 - 来快乐的摸鱼吧")
                .ShortDescription("摸鱼日记")
                .Usage("{0}moyu")
                .Example("{0}moyu")
                .Action(async (args) =>
                {
                    try
                    {
                        JObject resp = JObject.Parse(await _undefinedApi.Request.Get("https://api.vvhan.com/api/moyu?type=json"));
                        string? imUrl = resp.Value<string>("url");
                        if (imUrl != null)
                        {
                            await _undefinedApi.Api.SendGroupMsg(
                                            args.GroupId,
                                            _undefinedApi.GetMessageBuilder()
                                                .Image(imUrl, ImageSendType.Url, ImageSubType.Normal).Build()
                                        );
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                });
            _undefinedApi.RegisterCommand("bangumi")
                .Alias(["今日新番","新番"])
                .Description("当日番剧动漫更新列表")
                .ShortDescription("今日新番")
                .Usage("{0}bangumi")
                .Example("{0}bangumi")
                .Action(async (args) =>
                {
                    try
                    {
                        JObject resp = JObject.Parse(await _undefinedApi.Request.Get("https://xiaoapi.cn/API/zs_tf.php"));
                        BangumiCollection? bc = resp.Value<BangumiCollection>("data");
                        if (bc != null)
                        {
                            string outmsg = "";
                            string nbbili = (bc?.Bili ?? []).Aggregate("", (current, bi) => current + $"{bi.Time} [{bi.Title}] {bi.Updata}\n");
                            if (nbbili.Length > 0)
                            {
                                outmsg += ("----今日B站新番----\n" + nbbili);
                            }
                            string nbtx = (bc?.Tx ?? []).Aggregate("", (current, bi) => current + $"{bi.Time} [{bi.Title}] {bi.Updata}\n");
                            if (nbtx.Length > 0)
                            {
                                outmsg += ("----今日腾讯新番----\n" + nbtx);
                            }
                            await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text(outmsg)
                                    .Build()
                            );
                        }
                    }
                    catch
                    {
                        // ignored
                    }
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
            catch
            {
                // ignored
            }
        }
        private readonly Calendar _zhCnCalendar = new CultureInfo("zh-CN").DateTimeFormat.Calendar;

        private readonly Font _dateFont = new("Simsun", 60);

        private readonly Font _contentFont = new("Simsun", 40);

        private readonly SolidBrush _drawBrush = new(Color.Black);
        private string GenHistodayImage()
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"\n{_zhCnCalendar.GetDayOfWeek(DateTime.Now)}";
            string content = _undefinedApi.Request.Get("https://xiaoapi.cn/API/lssdjt.php").Result;
            Bitmap bg = new(1080, 1500);
            Graphics g = Graphics.FromImage(bg);
            g.Clear(Color.White);
            g.DrawString(currentTime, _dateFont, _drawBrush, new RectangleF(100, 100, 880, 200));
            g.DrawString(content, _contentFont, _drawBrush, new RectangleF(50, 400, 980, 1100));
            string imCachePath = Path.Join(_undefinedApi.CachePath, $"{DateTime.Now:HH-mm-ss}.png");
            bg.Save(imCachePath);
            g.Dispose();
            bg.Dispose();
            return imCachePath;
        }
    }

    internal struct BangumiInfo
    {
        [JsonProperty("id")]public int Id;
        [JsonProperty("cover")]public string Cover;
        [JsonProperty("title")]public string Title;
        [JsonProperty("time")]public string Time;
        [JsonProperty("updata")]public string Updata;
    }

    internal struct BangumiCollection
    {
        [JsonProperty("bili")]public List<BangumiInfo>? Bili;
        [JsonProperty("tx")]public List<BangumiInfo>? Tx;
    }
}
