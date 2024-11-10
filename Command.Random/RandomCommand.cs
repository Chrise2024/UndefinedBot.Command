using UndefinedBot.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UndefinedBot.Core.Utils;

namespace Command.Random
{
    public class RandomCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        public RandomCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("random")
                .Alias(["rand","随机"])
                .Description("随机图片\n支持种类：\nacg - ACG\ndog - 哈基汪\ncat - 哈基米\nfox - 狐狸\nstar - 星空\nbg - 壁纸")
                .ShortDescription("随机图片")
                .Usage("{0}random [PicType]")
                .Example("{0}random acg")
                .Action(async (commandContext) =>
                {
                    if (commandContext.Args.Param.Count > 0)
                    {
                        string outUrl = GetRandomContent(commandContext.Args.Param[0]);
                        if (outUrl.Length > 0)
                        {
                            await commandContext.Api.SendGroupMsg(
                                            commandContext.Args.GroupId,
                                            commandContext.GetMessageBuilder()
                                                .Image(outUrl, ImageSendType.Url).Build()
                                        );
                        }
                        else
                        {
                            await commandContext.Api.SendGroupMsg(
                                    commandContext.Args.GroupId,
                                    commandContext.GetMessageBuilder()
                                        .Text("呃啊，图片迷路了").Build()
                                );
                        }
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("Improper Arg: Too Less args");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
        private readonly System.Random _randomRoot = new();

        private string GetRandomContent(string randType)
        {
            if (randType.Equals("bg"))
            {
                return RandomBingWallPaper();
            }
            else if (randType.Equals("fox"))
            {
                return RandomFox();
            }
            else if (randType.Equals("cat"))
            {
                return RandomCat();
            }
            else if (randType.Equals("dog"))
            {
                return RandomDog();
            }
            else if (randType.Equals("acg"))
            {
                return RandomAcg();
            }
            else if (randType.Equals("star"))
            {
                return RandomStarrySky();
            }
            return "";
        }
        private string RandomBingWallPaper()
        {
            try
            {
                JObject resp = JObject.Parse(_undefinedApi.Request.Get($"https://www.bing.com/HPImageArchive.aspx?format=js&idx={_randomRoot.Next(0, 32767)}&n=1").Result);
                List<JObject> ia = resp["images"]?.ToObject<List<JObject>>() ?? [];
                if (ia.Count > 0)
                {
                    string uSuffix = ia[0].Value<string>("url") ?? "";
                    return uSuffix.Length > 0 ? $"https://www.bing.com{uSuffix}" : "";
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }
        private string RandomFox()
        {
            return $"https://randomfox.ca/images/{_randomRoot.Next(1, 124)}.jpg";
        }
        private string RandomCat()
        {
            try
            {
                if (_randomRoot.Next(1, 100) > 64)
                {
                    List<JObject> ia = JsonConvert.DeserializeObject<List<JObject>>(_undefinedApi.Request.Get("https://api.thecatapi.com/v1/images/search").Result) ?? [];
                    if (ia.Count > 0)
                    {
                        return ia[0].Value<string>("url") ?? "";
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    JObject resp = JObject.Parse(_undefinedApi.Request.Get("https://nekobot.xyz/api/image?type=neko").Result);
                    return resp.Value<string>("message") ?? "";
                }
            }
            catch
            {
                return "";
            }
        }
        private string RandomDog()
        {
            try
            {
                List<JObject> ia = JsonConvert.DeserializeObject<List<JObject>>(_undefinedApi.Request.Get("https://api.thedogapi.com/v1/images/search").Result) ?? [];
                if (ia.Count > 0)
                {
                    return ia[0].Value<string>("url") ?? "";
                }
                else
                {
                    JObject resp = JObject.Parse(_undefinedApi.Request.Get("https://dog.ceo/api/breeds/image/random").Result);
                    return resp.Value<string>("message") ?? "";
                }
            }
            catch
            {
                return "";
            }
        }
        private string RandomAcg()
        {
            try
            {
                if (_randomRoot.Next(1, 100) > 75)
                {
                    return _undefinedApi.Request.Get("https://www.loliapi.com/bg/?type=url").Result.Replace(".cn", ".com");
                }
                else
                {
                    JObject resp = JObject.Parse(_undefinedApi.Request.Get("https://iw233.cn/api.php?sort=cdniw&type=json").Result);
                    List<string> ia = resp["pic"]?.ToObject<List<string>>() ?? [];
                    return ia.Count > 0 ? ia[0] : "";
                }
            }
            catch
            {
                return "";
            }
        }
        private string RandomStarrySky()
        {
            try
            {
                JObject resp = JObject.Parse(_undefinedApi.Request.Get("https://moe.jitsu.top/api/?sort=starry&type=json").Result);
                List<string> ia = resp["pics"]?.ToObject<List<string>>() ?? [];
                return ia.Count > 0 ? ia[0] : "";
            }
            catch
            {
                return "";
            }
        }
    }
}
