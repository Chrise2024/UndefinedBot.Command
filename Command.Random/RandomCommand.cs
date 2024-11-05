using System;
using UndefinedBot.Core.Command;
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
                .Alias(["rand"])
                .Description("{0}random - 随机图片\n使用方法：{0}random PicType\nacg - ACG\ndog - 哈基汪\ncat - 哈基米\nfox - 狐狸\nstar - 星空\nbg - 壁纸")
                .ShortDescription("{0}random - 随机图片")
                .Example("{0}random acg")
                .Action(async (ArgSchematics args) =>
                {
                    if (args.Param.Count > 0)
                    {
                        string OutUrl = GetRandomContent(args.Param[0]);
                        if (OutUrl.Length > 0)
                        {
                            await _undefinedApi.Api.SendGroupMsg(
                                            args.GroupId,
                                            _undefinedApi.GetMessageBuilder()
                                                .Image(OutUrl, ImageSendType.Url).Build()
                                        );
                        }
                        else
                        {
                            await _undefinedApi.Api.SendGroupMsg(
                                    args.GroupId,
                                    _undefinedApi.GetMessageBuilder()
                                        .Text("呃啊，图片迷路了").Build()
                                );
                        }
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("random","Unproper Arg: Too Less args");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
        private readonly System.Random s_randomRoot = new();

        private string GetRandomContent(string RandType)
        {
            if (RandType.Equals("bg"))
            {
                return RandomBingWallPaper();
            }
            else if (RandType.Equals("fox"))
            {
                return RandomFox();
            }
            else if (RandType.Equals("cat"))
            {
                return RandomCat();
            }
            else if (RandType.Equals("dog"))
            {
                return RandomDog();
            }
            else if (RandType.Equals("acg"))
            {
                return RandomACG();
            }
            else if (RandType.Equals("star"))
            {
                return RandomStarrySky();
            }
            return "";
        }
        private string RandomBingWallPaper()
        {
            try
            {
                JObject Resp = JObject.Parse(_undefinedApi.Request.Get($"https://www.bing.com/HPImageArchive.aspx?format=js&idx={s_randomRoot.Next(0, 32767)}&n=1").Result);
                List<JObject> IA = Resp["images"]?.ToObject<List<JObject>>() ?? [];
                if (IA.Count > 0)
                {
                    string USuffix = IA[0].Value<string>("url") ?? "";
                    return USuffix.Length > 0 ? $"https://www.bing.com{USuffix}" : "";
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
            return $"https://randomfox.ca/images/{s_randomRoot.Next(1, 124)}.jpg";
        }
        private string RandomCat()
        {
            try
            {
                if (s_randomRoot.Next(1, 100) > 64)
                {
                    List<JObject> IA = JsonConvert.DeserializeObject<List<JObject>>(_undefinedApi.Request.Get("https://api.thecatapi.com/v1/images/search").Result) ?? [];
                    if (IA.Count > 0)
                    {
                        return IA[0].Value<string>("url") ?? "";
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    JObject Resp = JObject.Parse(_undefinedApi.Request.Get("https://nekobot.xyz/api/image?type=neko").Result);
                    return Resp.Value<string>("message") ?? "";
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
                List<JObject> IA = JsonConvert.DeserializeObject<List<JObject>>(_undefinedApi.Request.Get("https://api.thedogapi.com/v1/images/search").Result) ?? [];
                if (IA.Count > 0)
                {
                    return IA[0].Value<string>("url") ?? "";
                }
                else
                {
                    JObject Resp = JObject.Parse(_undefinedApi.Request.Get("https://dog.ceo/api/breeds/image/random").Result);
                    return Resp.Value<string>("message") ?? "";
                }
            }
            catch
            {
                return "";
            }
        }
        private string RandomACG()
        {
            try
            {
                if (s_randomRoot.Next(1, 100) > 75)
                {
                    return _undefinedApi.Request.Get("https://www.loliapi.com/bg/?type=url").Result.Replace(".cn", ".com");
                }
                else
                {
                    JObject Resp = JObject.Parse(_undefinedApi.Request.Get("https://iw233.cn/api.php?sort=cdniw&type=json").Result);
                    List<string> IA = Resp["pic"]?.ToObject<List<string>>() ?? [];
                    return IA.Count > 0 ? IA[0] : "";
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
                JObject Resp = JObject.Parse(_undefinedApi.Request.Get("https://moe.jitsu.top/api/?sort=starry&type=json").Result);
                List<string> IA = Resp["pics"]?.ToObject<List<string>>() ?? [];
                return IA.Count > 0 ? IA[0] : "";
            }
            catch
            {
                return "";
            }
        }
    }
}
