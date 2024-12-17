using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.CommandNodes;
using System.Globalization;
using System.Text;
using System.Text.Json;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Registry;

namespace Command.Mix;

public class MixCommand : IPluginInitializer
{
    private Dictionary<string, Dictionary<string, string>> MixMeta { get; set; } = [];
    public void Initialize(UndefinedApi undefinedApi)
    {
        MixMeta = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(Path.Join(undefinedApi.PluginPath,"data.json")) ?? throw new FileNotFoundException("data.json Not Exist")) ?? throw new NotImplementedException();
        undefinedApi.RegisterCommand("mix")
            .Alias(["mixemoji", "emojimix", "混合"])
            .Description("混合Emoji")
            .ShortDescription("混合Emoji")
            .Usage("mix [Emoji1] [Emoji2]")
            .Example("{0}mix 😀 😁")
            .Execute(async (ctx) =>
            {
                await ctx.Api.SendGroupMsg(
                    ctx.CallingProperties.GroupId,
                    ctx.GetMessageBuilder()
                        .Text("空气不能混合")
                        .Build()
                );
            }).Then(new VariableNode("emoji1", new StringArgument())
                .Execute(async (ctx) =>
                {
                    string mixRes = MixEmoji([StringArgument.GetString("emoji1", ctx)]);
                    if (mixRes.Length > 0)
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Reply(ctx.CallingProperties.MsgId)
                                .Image(mixRes, ImageSendType.Url)
                                .Build()
                        );
                    }
                    else
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("似乎不能混合")
                                .Build()
                        );
                    }
                }).Then(new VariableNode("emoji2",new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        string mixRes = MixEmoji([StringArgument.GetString("emoji1", ctx),StringArgument.GetString("emoji2", ctx)]);
                        if (mixRes.Length > 0)
                        {
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Reply(ctx.CallingProperties.MsgId)
                                    .Image(mixRes, ImageSendType.Url)
                                    .Build()
                            );
                        }
                        else
                        {
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Text("似乎不能混合")
                                    .Build()
                            );
                        }
                    })));

    }
    private string GetEmojiUnicodePoint(string emojiString)
    {
        try
        {
            if (emojiString.Length > 0)
            {
                StringRuneEnumerator stringRuneEnumerator = emojiString.EnumerateRunes();
                string tmp = "";
                foreach (Rune r in stringRuneEnumerator)
                {
                    tmp += $"u{r.Value:x}-";
                }
                return tmp.Length > 0 ? tmp[..^1] : "";
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
    private string MixEmoji(List<string> emojiStringArray)
    {
        string e1Cp;
        string e2Cp;
        if (emojiStringArray.Count == 1)
        {
            List<string> lineElement = [];
            TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(emojiStringArray[0]);
            elementEnumerator.Reset();
            while (elementEnumerator.MoveNext())
            {
                string currentElement = elementEnumerator.GetTextElement();
                lineElement.Add(currentElement);
            }
            if (lineElement.Count > 1)
            {

                e1Cp = GetEmojiUnicodePoint(lineElement[0]);
                e2Cp = GetEmojiUnicodePoint(lineElement[1]);
            }
            else if (lineElement.Count == 1)
            {

                e1Cp = GetEmojiUnicodePoint(lineElement[0]);
                e2Cp = GetEmojiUnicodePoint(lineElement[0]);
            }
            else
            {
                return "";
            }
        }
        else if (emojiStringArray.Count > 1)
        {
            e1Cp = GetEmojiUnicodePoint(emojiStringArray[0]);
            e2Cp = GetEmojiUnicodePoint(emojiStringArray[1]);
        }
        else
        {
            return "";
        }
        string? date = MixMeta.GetValueOrDefault(e1Cp)?.GetValueOrDefault(e2Cp);
        if (date != null)
        {
            return $"https://www.gstatic.com/android/keyboard/emojikitchen/{date}/{e1Cp}/{e1Cp}_{e2Cp}.png";
        }
        else
        {
            date = MixMeta.GetValueOrDefault(e2Cp)?.GetValueOrDefault(e1Cp);
            if (date != null)
            {
                return $"https://www.gstatic.com/android/keyboard/emojikitchen/{date}/{e2Cp}/{e2Cp}_{e1Cp}.png";
            }
            else
            {
                return "";
            }
        }
    }
    private bool IsEmoji(string textElement)
    {
        UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(textElement.Length > 0 ? textElement[0] : ' ');
        return uc == UnicodeCategory.OtherSymbol || uc == UnicodeCategory.ModifierSymbol ||
               uc == UnicodeCategory.PrivateUse || uc == UnicodeCategory.Surrogate;
    }
}
