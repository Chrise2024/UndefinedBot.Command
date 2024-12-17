using System.Text.Json;
using System.Text.Json.Nodes;
using UndefinedBot.Core;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Registry;
using UndefinedBot.Core.Utils;

namespace Command.Random;

public class RandomCommand : IPluginInitializer
{
    public void Initialize(UndefinedApi undefinedApi)
    {
        undefinedApi.RegisterCommand("random")
            .Alias(["rand", "随机"])
            .Description("随机图片\n支持种类：\nacg - ACG\ndog - 哈基汪\ncat - 哈基米\nfox - 狐狸\nstar - 星空\nbg - 壁纸")
            .ShortDescription("随机图片")
            .Usage("{0}random [PicType]")
            .Example("{0}random acg")
            .Execute(async (ctx) =>
            {
                await ctx.Api.SendGroupMsg(
                    ctx.CallingProperties.GroupId,
                    ctx.GetMessageBuilder()
                        .Text("随机了个啥").Build()
                );
            }).Then(new VariableNode("type", new StringArgument())
                .Execute(async (ctx) =>
                {
                    string outUrl = GetRandomContent(StringArgument.GetString("type", ctx),undefinedApi);
                    if (outUrl.Length > 0)
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Image(outUrl, ImageSendType.Url).Build()
                        );
                    }
                    else
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("呃啊，图片迷路了").Build()
                        );
                    }
                }));

    }
    private readonly System.Random _randomRoot = new();

    private string GetRandomContent(string randType,UndefinedApi undefinedApi)
    {
        if (randType.Equals("bg"))
        {
            return RandomBingWallPaper(undefinedApi);
        }
        else if (randType.Equals("fox"))
        {
            return RandomFox();
        }
        else if (randType.Equals("cat"))
        {
            return RandomCat(undefinedApi);
        }
        else if (randType.Equals("dog"))
        {
            return RandomDog(undefinedApi);
        }
        else if (randType.Equals("acg"))
        {
            return RandomAcg(undefinedApi);
        }
        else if (randType.Equals("star"))
        {
            return RandomStarrySky(undefinedApi);
        }
        return "";
    }
    private string RandomBingWallPaper(UndefinedApi undefinedApi)
    {
        try
        {
            JsonNode? resp = JsonNode.Parse(undefinedApi.Request.Get($"https://www.bing.com/HPImageArchive.aspx?format=js&idx={_randomRoot.Next(0, 32767)}&n=1").Result);
            List<JsonNode> ia = resp?["images"]?.Deserialize<List<JsonNode>>() ?? [];
            if (ia.Count > 0)
            {
                string uSuffix = ia[0]["url"]?.GetValue<string>() ?? "";
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
    private string RandomCat(UndefinedApi undefinedApi)
    {
        try
        {
            if (_randomRoot.Next(1, 100) > 64)
            {
                List<JsonNode> ia = JsonSerializer.Deserialize<List<JsonNode>>(undefinedApi.Request.Get("https://api.thecatapi.com/v1/images/search").Result) ?? [];
                if (ia.Count > 0)
                {
                    return ia[0]["url"]?.GetValue<string>() ?? "";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                JsonNode? resp = JsonNode.Parse(undefinedApi.Request.Get("https://nekobot.xyz/api/image?type=neko").Result);
                return resp?["message"]?.GetValue<string>() ?? "";
            }
        }
        catch
        {
            return "";
        }
    }
    private string RandomDog(UndefinedApi undefinedApi)
    {
        try
        {
            List<JsonNode> ia = JsonSerializer.Deserialize<List<JsonNode>>(undefinedApi.Request.Get("https://api.thedogapi.com/v1/images/search").Result) ?? [];
            if (ia.Count > 0)
            {
                return ia[0]["url"]?.GetValue<string>() ?? "";
            }
            else
            {
                JsonNode? resp = JsonNode.Parse(undefinedApi.Request.Get("https://dog.ceo/api/breeds/image/random").Result);
                return resp?["message"]?.GetValue<string>() ?? "";
            }
        }
        catch
        {
            return "";
        }
    }
    private string RandomAcg(UndefinedApi undefinedApi)
    {
        try
        {
            if (_randomRoot.Next(1, 100) > 75)
            {
                return undefinedApi.Request.Get("https://www.loliapi.com/bg/?type=url").Result.Replace(".cn", ".com");
            }
            else
            {
                JsonNode? resp = JsonNode.Parse(undefinedApi.Request.Get("https://iw233.cn/api.php?sort=cdniw&type=json").Result);
                List<string> ia = resp?["pic"]?.Deserialize<List<string>>() ?? [];
                return ia.Count > 0 ? ia[0] : "";
            }
        }
        catch
        {
            return "";
        }
    }
    private string RandomStarrySky(UndefinedApi undefinedApi)
    {
        try
        {
            JsonNode? resp = JsonNode.Parse(undefinedApi.Request.Get("https://moe.jitsu.top/api/?sort=starry&type=json").Result);
            List<string> ia = resp?["pics"]?.Deserialize<List<string>>() ?? [];
            return ia.Count > 0 ? ia[0] : "";
        }
        catch
        {
            return "";
        }
    }
}
