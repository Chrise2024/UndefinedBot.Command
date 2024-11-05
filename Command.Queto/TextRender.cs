using System.Globalization;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using SkiaSharp;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Command;
using Newtonsoft.Json;

namespace Command.Queto
{
    internal class TextRender
    {
        public static readonly Dictionary<string, string> QFaceReference = new()
        {
            { "0" , "😯" },
            { "1" , "😖" },
            { "2" , "😍" },
            { "3" , "😦" },
            { "4" , "😎" },
            { "5" , "😭" },
            { "6" , "😊" },
            { "7" , "🤐" },
            { "8" , "😪" },
            { "9" , "😢" },
            { "10" , "😟" },
            { "11" , "😡" },
            { "12" , "😋" },
            { "13" , "😁" },
            { "14" , "🙂" },
            { "15" , "🙁" },
            { "16" , "😎" },
            { "18" , "😩" },
            { "19" , "🤮" },
            { "20" , "🤭" },
            { "21" , "☺" },
            { "22" , "😶" },
            { "23" , "😕" },
            { "24" , "😋" },
            { "25" , "🥱" },
            { "26" , "😨" },
            { "27" , "😥" },
            { "31" , "🤬" },
            { "32" , "🤔" },
            { "36" , "🌚" },
            { "37" , "💀" },
            { "46" , "🐷" },
            { "53" , "🍰" },
            { "56" , "🔪" },
            { "59" , "💩" },
            { "60" , "☕️" },
            { "63" , "🌹" },
            { "64" , "🥀" },
            { "66" , "❤️" },
            { "67" , "💔" },
            { "74" , "🌞" },
            { "75" , "🌛" },
            { "76" , "👍" },
            { "77" , "👎" },
            { "78" , "🤝" },
            { "79" , "✌" },
            { "97" , "😓" },
            { "98" , "🥱" },
            { "110" , "😧" },
            { "111" , "🥺" },
            { "112" , "🔪" },
            { "114" , "🏀" },
            { "116" , "👄" },
            { "120" , "✊" },
            { "123" , "👆" },
            { "124" , "👌" },
            { "146" , "💢" },
            { "147" , "🍭" },
            { "171" , "🍵" },
            { "177" , "🤧" },
            { "182" , "😂" },
            { "185" , "🦙" },
            { "186" , "👻" },
            { "273" , "🍋" },
            { "325" , "😱" }
        };
        public static void GenTextImage(string tempFilePath, string text, int fontSize, int width, int height)
        {
            //Location:SKPoint->Bottom Center
            SKSurface Surface = SKSurface.Create(new SKImageInfo(width, height));
            SKCanvas Canvas = Surface.Canvas;
            Canvas.Clear(SKColors.Transparent);
            DrawTextWithWrapping(Canvas, text, new SKRect(0, 0, width, height), fontSize);
            SKImage TempImage = Surface.Snapshot();
            SKData TempData = TempImage.Encode(SKEncodedImageFormat.Png, 100);
            FileStream TempFileStream = File.OpenWrite(tempFilePath);
            if (TempFileStream != null)
            {
                TempData.SaveTo(TempFileStream);
                TempFileStream.Close();
            }
            TempData.Dispose();
            TempImage.Dispose();
            Canvas.Dispose();
            Surface.Dispose();
        }

        private static void DrawTextWithWrapping(SKCanvas canvas, string text, SKRect textArea, int fontSize)
        {
            SKPaint PaintEmoji = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Segoe UI Emoji"),
                TextSize = fontSize,
                Color = SKColors.White,
                IsAntialias = true,
                StrokeWidth = 1,
                TextAlign = SKTextAlign.Center,
                IsLinearText = true,
            };
            SKPaint PaintText = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Simsun"),
                TextSize = fontSize,
                Color = SKColors.White,
                IsAntialias = true,
                StrokeWidth = 1,
                TextAlign = SKTextAlign.Center,
                IsLinearText = true,
            };
            SKPoint DrawPosition = new(textArea.Width / 2, fontSize);
            List<List<string>> Lines = SplitString(text, fontSize, textArea.Width - fontSize * 1.5F, PaintText);
            float yOffset = (textArea.Height / 2) - (Lines.Count * fontSize / 2);
            for (int i = 0; i < Lines.Count; i++)
            {
                DrawLine(canvas, Lines[i], new SKPoint(DrawPosition.X, DrawPosition.Y + yOffset), PaintEmoji, PaintText);
                yOffset += fontSize;
                if (yOffset + fontSize > textArea.Height)
                {
                    break;
                }
            }
            PaintEmoji.Dispose();
            PaintText.Dispose();
        }
        private static void DrawLine(SKCanvas canvas, List<string> lineText, SKPoint linePosition, SKPaint paintEmoji, SKPaint paintText)
        {
            float FontSize = paintText.TextSize;
            float LineWidth = 0;
            foreach (string index in lineText)
            {
                LineWidth += IsEmoji(index) ? paintEmoji.MeasureText(index) : paintText.MeasureText(index);
            }
            float LPos = linePosition.X - LineWidth / 2 + FontSize / 2;
            foreach (string TE in lineText)
            {
                if (IsEmoji(TE))
                {
                    canvas.DrawText(TE, LPos + paintEmoji.MeasureText(TE) / 2, linePosition.Y, paintEmoji);
                    LPos += paintEmoji.MeasureText(TE);
                }
                else
                {
                    canvas.DrawText(TE, LPos + paintText.MeasureText(TE) / 2, linePosition.Y, paintText);
                    LPos += paintText.MeasureText(TE);
                }
            }
        }
        private static List<List<string>> SplitString(string input, int fontSize, float width, SKPaint currentPaint)
        {
            width -= fontSize;
            List<List<string>> output = [];
            List<string> TempLine = [];
            List<string> Elements = [];
            float CLength = 0;
            TextElementEnumerator ElementEnumerator = StringInfo.GetTextElementEnumerator(input);
            ElementEnumerator.Reset();
            while (ElementEnumerator.MoveNext())
            {
                Elements.Add(ElementEnumerator.GetTextElement());
            }
            if (Elements.Count > 1)
            {
                string TempString = Elements[0];
                for (int index = 1; index < Elements.Count; index++)
                {
                    if (IsEmoji(Elements[index - 1]) == IsEmoji(Elements[index]))
                    {
                        TempString += Elements[index];
                    }
                    else
                    {
                        TempLine.Add(TempString);
                        TempString = Elements[index];
                    }
                    if (IsEmoji(Elements[index - 1]))
                    {
                        CLength += currentPaint.TextSize * 1.5F;
                    }
                    else
                    {
                        CLength += currentPaint.MeasureText(Elements[index - 1]);
                    }
                    if (CLength > width)
                    {
                        if (TempString.Length > 0)
                        {
                            TempLine.Add(TempString);
                            TempString = "";
                        }
                        output.Add(TempLine);
                        TempLine = [];
                        CLength = 0;
                    }
                }
                if (TempString.Length > 0)
                {
                    TempLine.Add(TempString);
                    output.Add(TempLine);
                }
                return output;
            }
            else
            {
                return [[input]];
            }
        }
        private static bool IsEmoji(string textElement)
        {
            UnicodeCategory UC = CharUnicodeInfo.GetUnicodeCategory(textElement.Length > 0 ? textElement[0] : ' ');
            return UC == UnicodeCategory.OtherSymbol || UC == UnicodeCategory.ModifierSymbol ||
                   UC == UnicodeCategory.PrivateUse || UC == UnicodeCategory.Surrogate;
        }
        private static bool IsASCII(char inputChar)
        {
            return inputChar >= 0x00 && inputChar < 0xFF;
        }
        private static bool IsASCII(string textElement)
        {
            if (textElement.Length != 1)
            {
                return false;
            }
            else
            {
                return textElement[0] >= 0x00 && textElement[0] < 0xFF;
            }
        }
    }
    internal partial class RegexProvider
    {
        [GeneratedRegex(@"\[CQ:\S+\]")]
        public static partial Regex GetCQEntityRegex();

        [GeneratedRegex(@"\[CQ:at,qq=\d+\S*\]")]
        public static partial Regex GetCQAtRegex();

        [GeneratedRegex(@"^\[CQ:reply,id=[-]*\d+\]")]
        public static partial Regex GetCQReplyRegex();

        [GeneratedRegex(@"\d+")]
        public static partial Regex GetIdRegex();

        [GeneratedRegex(@"^\s+$")]
        public static partial Regex GetEmptyStringRegex();

        [GeneratedRegex(@"\*\(1\)|\+\(0\)$")]
        public static partial Regex GetEmptyMultipleEmelment();

        [GeneratedRegex(@"([\*|\/])\(([^\+\-\(\)]+)\)$")]
        public static partial Regex GetMultipleNumberEmelment();
    }
}
