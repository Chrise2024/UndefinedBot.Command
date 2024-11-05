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
                .Description("{0}mix - 混合Emoji\n使用方法：{0}mix Emoji1 Emoji2")
                .ShortDescription("{0}mix - 混合Emoji")
                .Example("{0}mix 😀 😁")
                .Action(async (ArgSchematics args) =>
                {
                    string MixRes = MixEmoji(args.Param);
                    if (MixRes.Length > 0)
                    {
                        await _undefinedApi.Api.SendGroupMsg(
                                        args.GroupId,
                                        _undefinedApi.GetMessageBuilder()
                                            .Reply(args.MsgId)
                                            .Image(MixRes, ImageSendType.Url).Build()
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
                    StringRuneEnumerator SRE = emojiString.EnumerateRunes();
                    foreach (Rune R in SRE)
                    {
                        return R.Value;
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
            int E1CP;
            int E2CP;
            if (emojiStringArray.Count == 1)
            {
                List<string> LineElement = [];
                TextElementEnumerator ElementEnumerator = StringInfo.GetTextElementEnumerator(emojiStringArray[0]);
                ElementEnumerator.Reset();
                while (ElementEnumerator.MoveNext())
                {
                    string CurrentElement = ElementEnumerator.GetTextElement();
                    if (IsEmoji(CurrentElement))
                    {
                        LineElement.Add(CurrentElement);
                    }
                }
                if (LineElement.Count > 1)
                {

                    E1CP = GetEmojiUnicodePoint(LineElement[0]);
                    E2CP = GetEmojiUnicodePoint(LineElement[1]);
                }
                else
                {
                    return "";
                }
            }
            else if (emojiStringArray.Count > 1)
            {
                E1CP = GetEmojiUnicodePoint(emojiStringArray[0]);
                E2CP = GetEmojiUnicodePoint(emojiStringArray[1]);
            }
            else
            {
                return "";
            }
            string TUrlN = $"https://www.gstatic.com/android/keyboard/emojikitchen/20201001/u{E1CP:x}/u{E1CP:x}_u{E2CP:x}.png";
            string TUrlR = $"https://www.gstatic.com/android/keyboard/emojikitchen/20201001/u{E2CP:x}/u{E2CP:x}_u{E1CP:x}.png";
            byte[] Res = _undefinedApi.Request.GetBinary(TUrlN).Result;
            if (Res.Length == 0 || Res[0] != 0x89)
            {
                Res = _undefinedApi.Request.GetBinary(TUrlR).Result;
                if (Res.Length == 0 || Res[0] != 0x89)
                {
                    return "";
                }
                else
                {
                    return TUrlR;
                }
            }
            else
            {
                return TUrlN;
            }
        }
        private bool IsEmoji(string textElement)
        {
            UnicodeCategory UC = CharUnicodeInfo.GetUnicodeCategory(textElement.Length > 0 ? textElement[0] : ' ');
            return UC == UnicodeCategory.OtherSymbol || UC == UnicodeCategory.ModifierSymbol ||
                   UC == UnicodeCategory.PrivateUse || UC == UnicodeCategory.Surrogate;
        }
    }
}