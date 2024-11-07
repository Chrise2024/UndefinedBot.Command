using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using System.Globalization;
using System.Text;

namespace Command.Template
{
    public class MixCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        public MixCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("mix")
                .Alias(["mixemoji", "emojimix"])
                .Description("混合Emoji")
                .ShortDescription("混合Emoji")
                .Usage("mix [Emoji1] [Emoji2]")
                .Example("{0}mix 😀 😁")
                .Action(async (args) =>
                {
                    string mixRes = MixEmoji(args.Param);
                    if (mixRes.Length > 0)
                    {
                        await _undefinedApi.Api.SendGroupMsg(
                                        args.GroupId,
                                        _undefinedApi.GetMessageBuilder()
                                            .Reply(args.MsgId)
                                            .Image(mixRes, ImageSendType.Url).Build()
                                    );
                    }
                    else
                    {
                        await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text("似乎不能混合").Build()
                            );
                    }
                });
            _undefinedApi.SubmitCommand();
        }
        private int GetEmojiUnicodePoint(string emojiString)
        {
            try
            {
                if (emojiString.Length > 0)
                {
                    StringRuneEnumerator stringRuneEnumerator = emojiString.EnumerateRunes();
                    foreach (Rune r in stringRuneEnumerator)
                    {
                        return r.Value;
                    }
                    return 0;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
        private string MixEmoji(List<string> emojiStringArray)
        {
            int e1Cp;
            int e2Cp;
            if (emojiStringArray.Count == 1)
            {
                List<string> lineElement = [];
                TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(emojiStringArray[0]);
                elementEnumerator.Reset();
                while (elementEnumerator.MoveNext())
                {
                    string currentElement = elementEnumerator.GetTextElement();
                    if (IsEmoji(currentElement))
                    {
                        lineElement.Add(currentElement);
                    }
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
        }
        private bool IsEmoji(string textElement)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(textElement.Length > 0 ? textElement[0] : ' ');
            return uc == UnicodeCategory.OtherSymbol || uc == UnicodeCategory.ModifierSymbol ||
                   uc == UnicodeCategory.PrivateUse || uc == UnicodeCategory.Surrogate;
        }
    }
}