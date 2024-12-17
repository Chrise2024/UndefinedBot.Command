using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UndefinedBot.Core;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Registry;

namespace Command.Word
{
    public class Base : IPluginInitializer
    {
        private readonly Random _randRoot = new();
        public void Initialize(UndefinedApi undefinedApi)
        {
            undefinedApi.RegisterCommand("hito")
                .Alias(["hitokoto", "一言", "随机一言"])
                .Description(
                    "随机一言\n类型对照：\na - 动画\nb - 漫画\nc - 游戏\nd - 文学\ne - 原创\nf - 来自网络\ng - 其他\nh - 影视\ni - 诗词\nj - 网易云\nk - 哲学\nl - 抖机灵")
                .ShortDescription("随机一言")
                .Usage("{0}hito [一言类型]，不填类型则随机")
                .Example("{0}hito b")
                .Execute(async (ctx) =>
                {
                    HitokotoSchematics hitokoto = await GetHitokoto(undefinedApi);
                    if (hitokoto.Id != 0)
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text($"{hitokoto.Hitokoto}\n---- {hitokoto.Creator}").Build()
                        );
                    }
                    else
                    {
                        undefinedApi.Logger.Error($"Get Hitokoto Failed");
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("一言似乎迷路了").Build()
                        );
                    }
                }).Then(new VariableNode("type", new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        HitokotoSchematics hitokoto = await GetHitokoto(undefinedApi,StringArgument.GetString("type",ctx));
                        if (hitokoto.Id != 0)
                        {
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Text($"{hitokoto.Hitokoto}\n---- {hitokoto.Creator}").Build()
                            );
                        }
                        else
                        {
                            undefinedApi.Logger.Error($"Get Hitokoto Failed");
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Text("一言似乎迷路了").Build()
                            );
                        }
                    }));
            undefinedApi.RegisterCommand("lovetext")
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
            undefinedApi.RegisterCommand("joke")
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
            undefinedApi.RegisterCommand("tg")
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
            undefinedApi.RegisterCommand("onset")
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
                        JsonNode? resp = JsonNode.Parse(await ctx.Request.Get($"https://xiaobapi.top/api/xb/api/onset.php?name={target}"));
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text(resp?["data"]?.GetValue<string>() ?? "发病失败")
                                .Build()
                            );
                    }));
            undefinedApi.RegisterCommand("nosence")
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
            undefinedApi.RegisterCommand("lzcydn")
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

        }
        private async Task<HitokotoSchematics> GetHitokoto(UndefinedApi undefinedApi,string hitoType = "")
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
                string response = await undefinedApi.Request.Get("https://v1.hitokoto.cn/?" + para);
                return JsonSerializer.Deserialize<HitokotoSchematics>(response) ?? new();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Canceled: ");
                undefinedApi.Logger.Error(ex.Message);
                undefinedApi.Logger.Error(ex.StackTrace ?? "");
                return new HitokotoSchematics();
            }
            catch
            {
                return new HitokotoSchematics();
            }
        }
    }
    internal class HitokotoSchematics
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("uuid")] public string Uuid { get; set; } = "";
        [JsonPropertyName("hitokoto")] public string Hitokoto { get; set; } = "";
        [JsonPropertyName("type")] public string Type { get; set; } = "";
        [JsonPropertyName("from")] public string From { get; set; } = "";
        [JsonPropertyName("from_who")] public string FromWho { get; set; } = "";
        [JsonPropertyName("creator")] public string Creator { get; set; } = "";
        [JsonPropertyName("creator_uid")] public int CreatorUid { get; set; }
        [JsonPropertyName("reviewer")] public int Reviewer { get; set; }
        [JsonPropertyName("commit_from")] public string CommitFrom { get; set; } = "";
        [JsonPropertyName("created_at")] public string CreatedAt { get; set; } = "";
        [JsonPropertyName("length")] public int Length { get; set; }
    }
}
