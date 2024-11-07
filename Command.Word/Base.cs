using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.NetWork;

namespace Command.Word
{
    public class Base
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        public Base(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("hito")
                .Alias(["hitokoto","一言","随机一言"])
                .Description("随机一言\n类型对照：\na - 动画\nb - 漫画\nc - 游戏\nd - 文学\ne - 原创\nf - 来自网络\ng - 其他\nh - 影视\ni - 诗词\nj - 网易云\nk - 哲学\nl - 抖机灵")
                .ShortDescription("随机一言")
                .Usage("{0}hito [一言类型]，不填类型则随机")
                .Example("{0}hito b")
                .Action(async (args) =>
                {
                    HitokotoSchematics hitokoto = await GetHitokoto(args.Param.Count == 0 ? "" : args.Param[0]);
                    if ((hitokoto.Id ?? 0) != 0)
                    {
                        await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text($"{hitokoto.Hitokoto}\n---- {hitokoto.Creator}").Build()
                            );
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("hito",$"Get Hitokoto Failed");
                        await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text("一言似乎迷路了").Build()
                            );
                    }
                });
            _undefinedApi.RegisterCommand("lovetext")
                .Alias(["情话", "来点情话"])
                .Description("随机情话")
                .ShortDescription("随机情话")
                .Usage("{0}lovetext")
                .Example("{0}lovetext")
                .Action(async (args) =>
                {
                    try
                    {
                        await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text(await _undefinedApi.Request.Get("https://api.vvhan.com/api/text/love")).Build()
                            );

                    }
                    catch
                    {
                        // ignored
                    }
                });
            _undefinedApi.RegisterCommand("joke")
                .Alias(["笑话", "来点笑话"])
                .Description("随机笑话")
                .ShortDescription("随机笑话")
                .Usage("{0}joke")
                .Example("{0}joke")
                .Action(async (args) =>
                {
                    try
                    {
                        await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text(await _undefinedApi.Request.Get("https://api.vvhan.com/api/text/joke")).Build()
                            );

                    }
                    catch
                    {
                        // ignored
                    }
                });
            _undefinedApi.RegisterCommand("tg")
                .Alias(["舔狗", "舔狗日记"])
                .Description("舔狗日记\n使用方法：")
                .ShortDescription("舔狗日记")
                .Usage("{0}tg")
                .Example("{0}tg")
                .Action(async (args) =>
                {
                    try
                    {
                        await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text(await _undefinedApi.Request.Get("https://api.vvhan.com/api/text/dog")).Build()
                            );

                    }
                    catch
                    {
                        // ignored
                    }
                });
            _undefinedApi.SubmitCommand();
        }
        private async Task<HitokotoSchematics> GetHitokoto(string htioType = "")
        {
            string para = "";
            foreach (char index in htioType)
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
                Console.WriteLine("Task Cacled: ");
                _undefinedApi.Logger.Error("hito", ex.Message);
                _undefinedApi.Logger.Error("hito", ex.StackTrace ?? "");
                return new HitokotoSchematics();
            }
            catch
            {
                return new HitokotoSchematics();
            }
        }
    }
}
