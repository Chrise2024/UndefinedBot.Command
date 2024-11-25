using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.NetWork;

namespace Command.Word
{
    public class Base
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        private readonly Random _randRoot = new();
        public Base(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("hito")
                .Alias(["hitokoto", "一言", "随机一言"])
                .Description(
                    "随机一言\n类型对照：\na - 动画\nb - 漫画\nc - 游戏\nd - 文学\ne - 原创\nf - 来自网络\ng - 其他\nh - 影视\ni - 诗词\nj - 网易云\nk - 哲学\nl - 抖机灵")
                .ShortDescription("随机一言")
                .Usage("{0}hito [一言类型]，不填类型则随机")
                .Example("{0}hito b")
                .Execute(async (ctx) =>
                {
                    HitokotoSchematics hitokoto = await GetHitokoto();
                    if ((hitokoto.Id ?? 0) != 0)
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text($"{hitokoto.Hitokoto}\n---- {hitokoto.Creator}").Build()
                        );
                    }
                    else
                    {
                        _undefinedApi.Logger.Error($"Get Hitokoto Failed");
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("一言似乎迷路了").Build()
                        );
                    }
                }).Then(new VariableNode("type", new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        HitokotoSchematics hitokoto = await GetHitokoto(StringArgument.GetString("type",ctx));
                        if ((hitokoto.Id ?? 0) != 0)
                        {
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Text($"{hitokoto.Hitokoto}\n---- {hitokoto.Creator}").Build()
                            );
                        }
                        else
                        {
                            _undefinedApi.Logger.Error($"Get Hitokoto Failed");
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Text("一言似乎迷路了").Build()
                            );
                        }
                    }));
            _undefinedApi.RegisterCommand("lovetext")
                .Alias(["情话", "来点情话"])
                .Description("随机情话")
                .ShortDescription("随机情话")
                .Usage("{0}lovetext")
                .Example("{0}lovetext")
                .Execute(async (ctx) =>
                {
                    string text = await ctx.Request.Get("https://api.vvhan.com/api/text/love");
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text(text.Length == 0 ? "获取失败" : text)
                            .Build()
                    );
                });
            _undefinedApi.RegisterCommand("joke")
                .Alias(["笑话", "来点笑话"])
                .Description("随机笑话")
                .ShortDescription("随机笑话")
                .Usage("{0}joke")
                .Example("{0}joke")
                .Execute(async (ctx) =>
                {
                    string text = await ctx.Request.Get("https://api.vvhan.com/api/text/joke");
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text(text.Length == 0 ? "获取失败" : text)
                            .Build()
                    );
                });
            _undefinedApi.RegisterCommand("tg")
                .Alias(["舔狗", "舔狗日记"])
                .Description("舔狗日记")
                .ShortDescription("舔狗日记")
                .Usage("{0}tg")
                .Example("{0}tg")
                .Execute(async (ctx) =>
                {
                    string text = await ctx.Request.Get("https://api.vvhan.com/api/text/dog");
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text(text.Length == 0 ? "获取失败" : text)
                            .Build()
                    );
                });
            _undefinedApi.RegisterCommand("onset")
                .Alias(["发病"])
                .Description("发病文案")
                .ShortDescription("发病")
                .Usage("{0}onset [发病对象]")
                .Example("{0}onset 哈基米")
                .Execute(async (ctx) =>
                {
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text("不能对奇怪的东西发病哦")
                            .Build()
                    );
                }).Then(new VariableNode("target",new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        string target = StringArgument.GetString("target", ctx);
                        JObject resp = JObject.Parse(await ctx.Request.Get($"https://xiaobapi.top/api/xb/api/onset.php?name={target}"));
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text(resp.Value<string>("data") ?? "发病失败")
                                .Build()
                            );
                    }));
            _undefinedApi.RegisterCommand("nosence")
                .Alias(["废话"])
                .Description("生成一篇废话文章")
                .ShortDescription("废话文学")
                .Usage("{0}nosence [标题]")
                .Example("{0}nosence Homo")
                .Execute(async (ctx) =>
                {
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text("文章得有个主题")
                            .Build()
                    );
                }).Then(new VariableNode("target",new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        string target = StringArgument.GetString("target", ctx);
                        string text = await ctx.Request.Get(
                            $"https://api.jkyai.top/API/gpbtwz/api.php?msg={target}&num={_randRoot.Next(150, 450)}&type=text");
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text(text.Length > 0 ? text : "生成失败")
                                .Build()
                        );
                    }));
            _undefinedApi.RegisterCommand("lzcydn")
                .Alias(["次元","二次元"])
                .Description("自己变成二次元少女是什么样的")
                .ShortDescription("来自次元的你")
                .Usage("{0}lzcydn [Name]")
                .Example("{0}lzcydn Homo")
                .Execute(async (ctx) =>
                {
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text("奇怪的东西可转生不了")
                            .Build()
                    );
                }).Then(new VariableNode("target",new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        string target = StringArgument.GetString("target", ctx);
                        string text = await ctx.Request.Get($"https://api.jkyai.top/API/lzcydn/api.php?name={target}&type=text");
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text(text.Length > 0 ? text : "生成失败")
                                .Build()
                        );
                    }));
            _undefinedApi.SubmitCommand();
        }
        private async Task<HitokotoSchematics> GetHitokoto(string hitoType = "")
        {
            string para = "";
            foreach (char index in hitoType)
            {
                if (index is >= 'a' and <= 'l')
                {
                    para += $"c={index}&";
                }
            }
            try
            {
                string response = await _undefinedApi.Request.Get("https://v1.hitokoto.cn/?" + para);
                return JsonConvert.DeserializeObject<HitokotoSchematics>(response);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Canceled: ");
                _undefinedApi.Logger.Error(ex.Message);
                _undefinedApi.Logger.Error(ex.StackTrace ?? "");
                return new HitokotoSchematics();
            }
            catch
            {
                return new HitokotoSchematics();
            }
        }
    }
    internal struct HitokotoSchematics
    {
        [JsonProperty("id")] public int? Id;
        [JsonProperty("uuid")] public string? Uuid;
        [JsonProperty("hitokoto")] public string? Hitokoto;
        [JsonProperty("type")] public string? Type;
        [JsonProperty("from")] public string? From;
        [JsonProperty("from_who")] public string? FromWho;
        [JsonProperty("creator")] public string? Creator;
        [JsonProperty("creator_uid")] public int? CreatorUid;
        [JsonProperty("reviewer")] public int? Reviewer;
        [JsonProperty("commit_from")] public string? CommitFrom;
        [JsonProperty("created_at")] public string? CreatedAt;
        [JsonProperty("length")] public int? Length;
    }
}
