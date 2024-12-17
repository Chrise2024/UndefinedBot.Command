using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Registry;

namespace Command.Information;

public class Base : IPluginInitializer
{
    private static readonly JsonSerializerOptions s_option = new() { WriteIndented = true };
    public void Initialize(UndefinedApi undefinedApi)
    {
        undefinedApi.RegisterCommand("histoday")
            .Description("历史上的今天")
            .ShortDescription("历史上的今天")
            .Usage("{0}histoday")
            .Example("{0}histoday")
            .Execute(async (ctx) =>
            {
                string imageCachePath = GenHistodayImage(undefinedApi);
                await ctx.Api.SendGroupMsg(
                    ctx.CallingProperties.GroupId,
                    ctx.GetMessageBuilder()
                        .Image(imageCachePath)
                        .Build()
                );
            });
        undefinedApi.RegisterCommand("moyu")
            .Alias(["摸鱼", "摸鱼日记"])
            .Description("摸鱼日记 - 来快乐的摸鱼吧")
            .ShortDescription("摸鱼日记")
            .Usage("{0}moyu")
            .Example("{0}moyu")
            .Execute(async (ctx) =>
            {
                try
                {
                    JsonNode? resp = JsonNode.Parse(await ctx.Request.Get("https://api.vvhan.com/api/moyu?type=json"));
                    string? imUrl = resp?["url"]?.GetValue<string>();
                    if (imUrl != null)
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Image(imUrl, ImageSendType.Url)
                                .Build()
                        );
                    }
                }
                catch(Exception ex)
                {
                    undefinedApi.Logger.Error("Error Occured, Error Information:");
                    undefinedApi.Logger.Error(ex.Message);
                    undefinedApi.Logger.Error(ex.StackTrace ?? "");
                }
            });
        undefinedApi.RegisterCommand("bangumi")
            .Alias(["今日新番", "新番"])
            .Description("当日番剧动漫更新列表")
            .ShortDescription("今日新番")
            .Usage("{0}bangumi")
            .Example("{0}bangumi")
            .Execute(async (ctx) =>
            {
                try
                {
                    JsonNode? resp = JsonNode.Parse(await ctx.Request.Get("https://xiaoapi.cn/API/zs_tf.php"));
                    BangumiCollection? bc = resp?["data"].Deserialize<BangumiCollection>();
                    if (bc != null)
                    {
                        string outmsg = "";
                        string nbbili = bc.Bili
                            .Aggregate(
                                "",
                                (current, bi) => current + $"{bi.Time} [{bi.Title}] {bi.Updata}\n"
                                    );
                        if (nbbili.Length > 0)
                        {
                            outmsg += ("----今日B站新番----\n" + nbbili);
                        }

                        string nbtx = (bc.Tx).Aggregate("",
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
                    undefinedApi.Logger.Error("Error Occured, Error Information:");
                    undefinedApi.Logger.Error(ex.Message);
                    undefinedApi.Logger.Error(ex.StackTrace ?? "");
                }
            });
        undefinedApi.RegisterCommand("raw")
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
                    MsgBody? targetMsg = await ctx.Api.GetMsg(ReplyArgument.GetQReply("target",ctx).MsgId);
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text(JsonSerializer.Serialize(targetMsg?.Message ?? [], s_option)).Build()
                    );
                }));

    }
    private readonly Calendar _zhCnCalendar = new CultureInfo("zh-CN").DateTimeFormat.Calendar;

    private readonly Font _dateFont = new("Simsun", 60);

    private readonly Font _contentFont = new("Simsun", 40);

    private readonly SolidBrush _drawBrush = new(Color.Black);
    private string GenHistodayImage(UndefinedApi undefinedApi)
    {
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"\n{_zhCnCalendar.GetDayOfWeek(DateTime.Now)}";
        string content = undefinedApi.Request.Get("https://xiaoapi.cn/API/lssdjt.php").Result;
        Bitmap bg = new(1080, 1500);
        Graphics g = Graphics.FromImage(bg);
        g.Clear(Color.White);
        g.DrawString(currentTime, _dateFont, _drawBrush, new RectangleF(100, 100, 880, 200));
        g.DrawString(content, _contentFont, _drawBrush, new RectangleF(50, 400, 980, 1100));
        string imCacheName = $"HD{DateTime.Now:yyyy-MM-dd}.png";
        string imCachePath = undefinedApi.Cache.GetFile(imCacheName);
        if (imCachePath.Length != 0)
        {
            return imCachePath;
        }
        else
        {
            imCachePath = undefinedApi.Cache.AddFile(imCacheName, imCacheName, 60 * 60 * 24);
        }
        bg.Save(imCachePath);
        g.Dispose();
        bg.Dispose();
        return imCachePath;
    }
}

[Serializable] internal class BangumiInfo
{
    [JsonPropertyName("id")]public int Id { get; set; }
    [JsonPropertyName("cover")] public string Cover { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("time")]public string Time { get; set; } = "";
    [JsonPropertyName("updata")]public string Updata { get; set; } = "";
}

[Serializable] internal class BangumiCollection
{
    [JsonPropertyName("bili")]public List<BangumiInfo> Bili { get; set; } = [];
    [JsonPropertyName("tx")]public List<BangumiInfo> Tx { get; set; } = [];
}
