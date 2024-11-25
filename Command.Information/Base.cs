using System.Drawing;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

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
                .Execute(async (ctx) =>
                {
                    string imageCachePath = GenHistodayImage();
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Image(imageCachePath, ImageSendType.LocalFile, ImageSubType.Normal)
                            .Build()
                    );
                });
            _undefinedApi.RegisterCommand("moyu")
                .Alias(["摸鱼", "摸鱼日记"])
                .Description("摸鱼日记 - 来快乐的摸鱼吧")
                .ShortDescription("摸鱼日记")
                .Usage("{0}moyu")
                .Example("{0}moyu")
                .Execute(async (ctx) =>
                {
                    try
                    {
                        JObject resp = JObject.Parse(await ctx.Request.Get("https://api.vvhan.com/api/moyu?type=json"));
                        string? imUrl = resp.Value<string>("url");
                        if (imUrl != null)
                        {
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Image(imUrl, ImageSendType.Url, ImageSubType.Normal)
                                    .Build()
                            );
                        }
                    }
                    catch(Exception ex)
                    {
                        _undefinedApi.Logger.Error("Error Occured, Error Information:");
                        _undefinedApi.Logger.Error(ex.Message);
                        _undefinedApi.Logger.Error(ex.StackTrace ?? "");
                    }
                });
            _undefinedApi.RegisterCommand("bangumi")
                .Alias(["今日新番", "新番"])
                .Description("当日番剧动漫更新列表")
                .ShortDescription("今日新番")
                .Usage("{0}bangumi")
                .Example("{0}bangumi")
                .Execute(async (ctx) =>
                {
                    try
                    {
                        JObject resp = JObject.Parse(await ctx.Request.Get("https://xiaoapi.cn/API/zs_tf.php"));
                        BangumiCollection? bc = resp["data"]?.ToObject<BangumiCollection>();
                        if (bc != null)
                        {
                            string outmsg = "";
                            string nbbili = (bc?.Bili ?? []).Aggregate("",
                                (current, bi) => current + $"{bi.Time} [{bi.Title}] {bi.Updata}\n");
                            if (nbbili.Length > 0)
                            {
                                outmsg += ("----今日B站新番----\n" + nbbili);
                            }

                            string nbtx = (bc?.Tx ?? []).Aggregate("",
                                (current, bi) => current + $"{bi.Time} [{bi.Title}] {bi.Updata}\n");
                            if (nbtx.Length > 0)
                            {
                                outmsg += ("----今日腾讯新番----\n" + nbtx);
                            }

                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Text(outmsg)
                                    .Build()
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _undefinedApi.Logger.Error("Error Occured, Error Information:");
                        _undefinedApi.Logger.Error(ex.Message);
                        _undefinedApi.Logger.Error(ex.StackTrace ?? "");
                    }
                });
            _undefinedApi.RegisterCommand("raw")
                .Description("{0}raw - 群u到底发的什么东西")
                .ShortDescription("{0}raw - 原始消息")
                .Usage("用{0}raw 回复想生成的消息")
                .Example("{0}raw")
                .Execute(async (ctx) =>
                {
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text("不知道这条消息在哪")
                            .Build()
                    );
                }).Then(new VariableNode("target", new ReplyArgument())
                    .Execute(async (ctx) =>
                    {
                        MsgBody targetMsg = await ctx.Api.GetMsg(ReplyArgument.GetQReply("target",ctx).MsgId);
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text(JsonConvert.SerializeObject(targetMsg.Message, Formatting.Indented)).Build()
                        );
                    }));
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
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
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
            string imCacheName = $"HD{DateTime.Now:yyyy-MM-dd}.png";
            string imCachePath = _undefinedApi.Cache.GetFile(imCacheName);
            if (imCachePath.Length != 0)
            {
                return imCachePath;
            }
            else
            {
                imCachePath = _undefinedApi.Cache.AddFile(imCacheName, imCacheName, 60 * 60 * 24);
            }
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
        [JsonProperty("bili")]public List<BangumiInfo> Bili;
        [JsonProperty("tx")]public List<BangumiInfo> Tx;
    }
}
