using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace Command.Template
{
    public class MixCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        private readonly Dictionary<string, Dictionary<string, string>> _mixMeta;
        public MixCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _mixMeta = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(Path.Join(_undefinedApi.PluginPath,"data.json")) ?? throw new FileNotFoundException("data.json Not Exist")) ?? throw new NotImplementedException();
            _undefinedApi.RegisterCommand("mix")
                .Alias(["mixemoji", "emojimix","混合"])
                .Description("混合Emoji")
                .ShortDescription("混合Emoji")
                .Usage("mix [Emoji1] [Emoji2]")
                .Example("{0}mix 😀 😁")
                .Action(async (commandContext) =>
                {
                    string mixRes = MixEmoji(commandContext.Args.Param);
                    if (mixRes.Length > 0)
                    {
                        await commandContext.Api.SendGroupMsg(
                                        commandContext.Args.GroupId,
                                        commandContext.GetMessageBuilder()
                                            .Reply(commandContext.Args.MsgId)
                                            .Image(mixRes, ImageSendType.Url).Build()
                                    );
                    }
                    else
                    {
                        await commandContext.Api.SendGroupMsg(
                                commandContext.Args.GroupId,
                                commandContext.GetMessageBuilder()
                                    .Text("似乎不能混合").Build()
                            );
                    }
                });
            _undefinedApi.SubmitCommand();
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
            string? date = _mixMeta.GetValueOrDefault(e1Cp)?.GetValueOrDefault(e2Cp);
            if (date != null)
            {
                return $"https://www.gstatic.com/android/keyboard/emojikitchen/{date}/{e1Cp}/{e1Cp}_{e2Cp}.png";
            }
            else
            {
                date = _mixMeta.GetValueOrDefault(e2Cp)?.GetValueOrDefault(e1Cp);
                if (date != null)
                {
                    return $"https://www.gstatic.com/android/keyboard/emojikitchen/{date}/{e2Cp}/{e2Cp}_{e1Cp}.png";
                }
                else
                {
                    return "";
                }
            }
            /*
            string urlN = $"https://www.gstatic.com/android/keyboard/emojikitchen/20201001/u{e1Cp:x}/u{e1Cp:x}_u{e2Cp:x}.png";
            string urlR = $"https://www.gstatic.com/android/keyboard/emojikitchen/20201001/u{e2Cp:x}/u{e2Cp:x}_u{e1Cp:x}.png";
            byte[] res = _undefinedApi.Request.GetBinary(urlN).Result;
            if (res.Length == 0 || res[0] != 0x89)
            {
                res = _undefinedApi.Request.GetBinary(urlR).Result;
                if (res.Length == 0 || res[0] != 0x89)
                {
                    return "";
                }
                else
                {
                    return urlR;
                }
            }
            else
            {
                return urlN;
            }
            */
        }
        private bool IsEmoji(string textElement)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(textElement.Length > 0 ? textElement[0] : ' ');
            return uc == UnicodeCategory.OtherSymbol || uc == UnicodeCategory.ModifierSymbol ||
                   uc == UnicodeCategory.PrivateUse || uc == UnicodeCategory.Surrogate;
        }
    }
}