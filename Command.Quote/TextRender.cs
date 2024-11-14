using System.Globalization;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace Command.Quote
{
    abstract internal class TextRender
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
            SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            DrawTextWithWrapping(canvas, text, new SKRect(0, 0, width, height), fontSize);
            SKImage tempImage = surface.Snapshot();
            SKData tempData = tempImage.Encode(SKEncodedImageFormat.Png, 100);
            FileStream tempFileStream = File.OpenWrite(tempFilePath);
            tempData.SaveTo(tempFileStream);
            tempFileStream.Close();
            tempData.Dispose();
            tempImage.Dispose();
            canvas.Dispose();
            surface.Dispose();
        }

        private static void DrawTextWithWrapping(SKCanvas canvas, string text, SKRect textArea, int fontSize)
        {
            SKPaint paintEmoji = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Segoe UI Emoji"),
                TextSize = fontSize,
                Color = SKColors.White,
                IsAntialias = true,
                StrokeWidth = 1,
                TextAlign = SKTextAlign.Center,
                IsLinearText = true,
            };
            SKPaint paintText = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Simsun"),
                TextSize = fontSize,
                Color = SKColors.White,
                IsAntialias = true,
                StrokeWidth = 1,
                TextAlign = SKTextAlign.Center,
                IsLinearText = true,
            };
            SKPoint drawPosition = new(textArea.Width / 2.0F, fontSize);
            List<List<string>> lines = SplitString(text, fontSize, textArea.Width - fontSize * 1.5F, paintText);
            float yOffset = (textArea.Height / 2.0F) - (lines.Count * fontSize / 2.0F);
            foreach (List<string> t in lines)
            {
                DrawLine(canvas, t, new SKPoint(drawPosition.X, drawPosition.Y + yOffset), paintEmoji, paintText);
                yOffset += fontSize;
                if (yOffset + fontSize > textArea.Height)
                {
                    break;
                }
            }
            paintEmoji.Dispose();
            paintText.Dispose();
        }
        private static void DrawLine(SKCanvas canvas, List<string> lineText, SKPoint linePosition, SKPaint paintEmoji, SKPaint paintText)
        {
            float fontSize = paintText.TextSize;
            float lineWidth = 0;
            foreach (string index in lineText)
            {
                lineWidth += IsEmoji(index) ? paintEmoji.MeasureText(index) : paintText.MeasureText(index);
            }
            float lPos = linePosition.X - lineWidth / 2.0F + fontSize / 2.0F;
            foreach (string te in lineText)
            {
                if (IsEmoji(te))
                {
                    canvas.DrawText(te, lPos + paintEmoji.MeasureText(te) / 2.0F, linePosition.Y, paintEmoji);
                    lPos += paintEmoji.MeasureText(te);
                }
                else
                {
                    canvas.DrawText(te, lPos + paintText.MeasureText(te) / 2.0F, linePosition.Y, paintText);
                    lPos += paintText.MeasureText(te);
                }
            }
        }
        private static List<List<string>> SplitString(string input, int fontSize, float width, SKPaint currentPaint)
        {
            width -= fontSize;
            List<List<string>> output = [];
            List<string> tempLine = [];
            List<string> elements = [];
            float cLength = 0;
            TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(input);
            elementEnumerator.Reset();
            while (elementEnumerator.MoveNext())
            {
                elements.Add(elementEnumerator.GetTextElement());
            }
            if (elements.Count > 1)
            {
                string tempString = elements[0];
                for (int index = 1; index < elements.Count; index++)
                {
                    if (IsEmoji(elements[index - 1]) == IsEmoji(elements[index]))
                    {
                        tempString += elements[index];
                    }
                    else
                    {
                        tempLine.Add(tempString);
                        tempString = elements[index];
                    }
                    if (IsEmoji(elements[index - 1]))
                    {
                        cLength += currentPaint.TextSize * 1.5F;
                    }
                    else
                    {
                        cLength += currentPaint.MeasureText(elements[index - 1]);
                    }
                    if (cLength > width)
                    {
                        if (tempString.Length > 0)
                        {
                            tempLine.Add(tempString);
                            tempString = "";
                        }
                        output.Add(tempLine);
                        tempLine = [];
                        cLength = 0;
                    }
                }
                if (tempString.Length > 0)
                {
                    tempLine.Add(tempString);
                    output.Add(tempLine);
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
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(textElement.Length > 0 ? textElement[0] : ' ');
            return uc == UnicodeCategory.OtherSymbol || uc == UnicodeCategory.ModifierSymbol ||
                   uc == UnicodeCategory.PrivateUse || uc == UnicodeCategory.Surrogate;
        }
    }
    internal partial class RegexProvider
    {
        [GeneratedRegex(@"\[CQ:\S+\]")]
        public static partial Regex GetCqEntityRegex();

        [GeneratedRegex(@"\[CQ:at,qq=\d+\S*\]")]
        public static partial Regex GetCqAtRegex();

        [GeneratedRegex(@"^\[CQ:reply,id=[-]*\d+\]")]
        public static partial Regex GetCqReplyRegex();

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
