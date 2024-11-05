using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.NetWork;
using Newtonsoft.Json;

namespace Command.Hitokoto
{
    public class HitoCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        public HitoCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("hito")
                .Alias(["hitokoto"])
                .Description("{0}hito - 随机一言\n使用方法：{0}hito [一言类型]，不填类型则随机\n类型对照：\na - 动画\nb - 漫画\nc - 游戏\nd - 文学\ne - 原创\nf - 来自网络\ng - 其他\nh - 影视\ni - 诗词\nj - 网易云\nk - 哲学\nl - 抖机灵")
                .ShortDescription("{0}hito - 随机一言")
                .Example("{0}hito b")
                .Action(async (ArgSchematics args) =>
                {
                    HitokotoSchematics Hitokoto = await GetHitokoto(args.Param.Count == 0 ? "" : args.Param[0]);
                    if ((Hitokoto.Id ?? 0) != 0)
                    {
                        await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text($"{Hitokoto.Hitokoto}\n---- {Hitokoto.Creator}").Build()
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
            _undefinedApi.SubmitCommand();
        }
        private async Task<HitokotoSchematics> GetHitokoto(string htioType = "")
        {
            string Para = "";
            foreach (char index in htioType)
            {
                if (index >= 'a' && index <= 'l')
                {
                    Para += $"c={index}&";
                }
            }
            try
            {
                string response = await _undefinedApi.Request.Get("https://v1.hitokoto.cn/?" + Para);
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
