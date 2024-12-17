using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Text.Json.Nodes;
using ImageMagick;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;

namespace Command.Symmet
{
    public enum ImageContentType
    {
        Url = 0,
        MsgId = 1,
    }
    internal class ImageConverter(UndefinedApi imageApi)
    {
        private readonly UndefinedApi _imageApi = imageApi;
        public string GetConvertedImage(string imageContent, ImageContentType contentType, string convertMethod = "L")
        {
            Image im;
            MemoryStream ms;
            if (contentType == ImageContentType.Url)
            {
                byte[] imageBytes = _imageApi.Request.GetBinary(imageContent).Result;
                ms = new MemoryStream(imageBytes);
                im = Image.FromStream(ms);
            }
            else
            {
                MsgBody? targetMsg = _imageApi.Api.GetMsg(imageContent).Result;
                if (targetMsg == null || targetMsg.MessageId == 0)
                {
                    return "";
                }

                byte[] imageBytes = _imageApi.Request.GetBinary(ExtractUrlFromMsg(targetMsg)).Result;
                ms = new MemoryStream(imageBytes);
                im = Image.FromStream(ms);
            }
            if (im.RawFormat.Equals(ImageFormat.Gif))
            {
                string imCachePath = Path.Join(_imageApi.CachePath, $"{DateTime.Now:HH-mm-ss}.gif");
                MagickImageCollection resultImage = GifConvert.GifTransform(im, convertMethod);
                if (resultImage.Count > 0)
                {
                    resultImage.Write(imCachePath);
                    resultImage.Dispose();
                    im.Dispose();
                    ms.Close();
                    return imCachePath;
                }
                else
                {
                    im.Dispose();
                    ms.Close();
                    return "";
                }
            }
            else
            {
                string imCachePath = Path.Join(_imageApi.CachePath, $"{DateTime.Now:HH-mm-ss}.png");
                Bitmap resultImage = PicConvert.PicTransform(new Bitmap(im), convertMethod);
                resultImage.Save(imCachePath, ImageFormat.Gif);
                resultImage.Dispose();
                im.Dispose();
                ms.Close();
                return imCachePath;
            }
        }
        private string ExtractUrlFromMsg(MsgBody msgBody)
        {
            if (!(msgBody.Message?.Count > 0))
            {
                return "";
            }

            List<JsonNode> msgChain = msgBody.Message;
            if (msgChain.Count <= 0)
            {
                return "";
            }

            JsonNode msg = msgChain[0];
            if (!(msg["type"]?.GetValue<string>().Equals("image") ?? false))
            {
                return "";
            }

            JsonNode? jt = msg["data"];
            if (jt == null)
            {
                return "";
            }

            JsonNode? dataObj = jt.Deserialize<JsonNode>();
            if (dataObj == null)
            {
                return "";
            }

            JsonNode? temp  = dataObj["url"];
            return temp != null ? temp.Deserialize<string>() ?? "" : dataObj["file"]?.GetValue<string>() ?? "";
        }
    }
    internal abstract class ImageSymmetry
    {
        public static Bitmap SymmetryL(Bitmap bmp)
        {
            int rw = bmp.Width / 2;
            Rectangle cropRect = new(0, 0, rw, bmp.Height);
            Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);
            Bitmap bg = new(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(croppedImage, new Point(0, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipY);
            g.DrawImage(croppedImage, new Point(rw, 0));
            bmp.Dispose();
            croppedImage.Dispose();
            return bg;
        }
        public static Bitmap SymmetryR(Bitmap bmp)
        {
            int rw = bmp.Width / 2;
            Rectangle cropRect = new(rw, 0, rw, bmp.Height);
            Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);
            Bitmap bg = new(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(croppedImage, new Point(rw, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipY);
            g.DrawImage(croppedImage, new Point(0, 0));
            bmp.Dispose();
            croppedImage.Dispose();
            return bg;
        }
        public static Bitmap SymmetryU(Bitmap bmp)
        {
            int rh = bmp.Height / 2;
            Rectangle cropRect = new(0, 0, bmp.Width, rh);
            Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);
            Bitmap bg = new(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(croppedImage, new Point(0, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(0, rh));
            bmp.Dispose();
            croppedImage.Dispose();
            return bg;
        }
        public static Bitmap SymmetryD(Bitmap bmp)
        {
            int rh = bmp.Height / 2;
            Rectangle cropRect = new(0, rh, bmp.Width, rh);
            Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);
            Bitmap bg = new(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(croppedImage, new Point(0, rh));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(0, 0));
            bmp.Dispose();
            croppedImage.Dispose();
            return bg;
        }
        public static Bitmap SymmetryLu(Bitmap bmp)
        {
            int rw = bmp.Width / 2;
            int rh = bmp.Height / 2;
            Rectangle cropRect = new(0, 0, rw, rh);
            Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);
            Bitmap bg = new(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(croppedImage, new Point(0, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(0, rh));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipY);
            g.DrawImage(croppedImage, new Point(rw, rh));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(rw, 0));
            bmp.Dispose();
            croppedImage.Dispose();
            return bg;
        }
        public static Bitmap SymmetryRu(Bitmap bmp)
        {
            int rw = bmp.Width / 2;
            int rh = bmp.Height / 2;
            Rectangle cropRect = new(rw, 0, rw, rh);
            Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);
            Bitmap bg = new(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(croppedImage, new Point(rw, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(rw, rh));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipY);
            g.DrawImage(croppedImage, new Point(0, rh));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(0, 0));
            bmp.Dispose();
            croppedImage.Dispose();
            return bg;
        }
        public static Bitmap SymmetryLd(Bitmap bmp)
        {
            int rw = bmp.Width / 2;
            int rh = bmp.Height / 2;
            Rectangle cropRect = new(0, rh, rw, rh);
            Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);
            Bitmap bg = new(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(croppedImage, new Point(0, rh));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(0, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipY);
            g.DrawImage(croppedImage, new Point(rw, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(rw, rh));
            bmp.Dispose();
            croppedImage.Dispose();
            return bg;
        }
        public static Bitmap SymmetryRd(Bitmap bmp)
        {
            int rw = bmp.Width / 2;
            int rh = bmp.Height / 2;
            Rectangle cropRect = new(rw, rh, rw, rh);
            Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);
            Bitmap bg = new(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(croppedImage, new Point(rw, rh));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(rw, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipY);
            g.DrawImage(croppedImage, new Point(0, 0));
            croppedImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            g.DrawImage(croppedImage, new Point(0, rh));
            bmp.Dispose();
            croppedImage.Dispose();
            return bg;
        }
    }
    internal abstract class PicConvert
    {
        public static Bitmap PicTransform(Bitmap picImage, string method)
        {
            var transformMethod = ImageSymmetry.SymmetryL;
            if (method.Equals("右左"))
            {
                transformMethod = ImageSymmetry.SymmetryR;
            }
            else if (method.Equals("上下"))
            {
                transformMethod = ImageSymmetry.SymmetryU;
            }
            else if (method.Equals("下上"))
            {
                transformMethod = ImageSymmetry.SymmetryD;
            }
            else if (method.Equals("左上"))
            {
                transformMethod = ImageSymmetry.SymmetryLu;
            }
            else if (method.Equals("左下"))
            {
                transformMethod = ImageSymmetry.SymmetryLd;
            }
            else if (method.Equals("右上"))
            {
                transformMethod = ImageSymmetry.SymmetryRu;
            }
            else if (method.Equals("右下"))
            {
                transformMethod = ImageSymmetry.SymmetryRd;
            }
            return transformMethod(picImage);
        }
    }
    internal abstract class GifConvert
    {

        private static readonly byte[] s_defaultBytes = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0];

        private static uint GetGifFrameDelay(Image image)
        {
            try
            {
                return (uint)BitConverter.ToInt32(image.GetPropertyItem(0x5100)?.Value ?? s_defaultBytes, 4);
            }
            catch
            {
                return 0;
            }
        }
        public static MagickImageCollection GifTransform(Image gifImage, string method)
        {
            var transformMethod = ImageSymmetry.SymmetryL;
            if (method.Equals("右左"))
            {
                transformMethod = ImageSymmetry.SymmetryR;
            }
            else if (method.Equals("上下"))
            {
                transformMethod = ImageSymmetry.SymmetryU;
            }
            else if (method.Equals("下上"))
            {
                transformMethod = ImageSymmetry.SymmetryD;
            }
            else if (method.Equals("左上"))
            {
                transformMethod = ImageSymmetry.SymmetryLu;
            }
            else if (method.Equals("左下"))
            {
                transformMethod = ImageSymmetry.SymmetryLd;
            }
            else if (method.Equals("右上"))
            {
                transformMethod = ImageSymmetry.SymmetryRu;
            }
            else if (method.Equals("右下"))
            {
                transformMethod = ImageSymmetry.SymmetryRd;
            }

            FrameDimension dimension = new(gifImage.FrameDimensionsList[0]);
            int frameCount = gifImage.GetFrameCount(dimension);
            uint delay = GetGifFrameDelay(gifImage);
            var ncollection = new MagickImageCollection();
            for (int i = 0; i < frameCount; i++)
            {
                gifImage.SelectActiveFrame(dimension, i);
                Bitmap frame = new(gifImage);
                MemoryStream fMemoryStream = new();
                transformMethod(frame).Save(fMemoryStream, ImageFormat.Bmp);
                fMemoryStream.Position = 0;
                MagickImage magickFrame = new(fMemoryStream)
                {
                    AnimationDelay = delay,
                    GifDisposeMethod = GifDisposeMethod.Background
                };
                ncollection.Add(magickFrame);
                fMemoryStream.Close();
                frame.Dispose();
            }
            ncollection[0].AnimationIterations = 0;
            gifImage.Dispose();
            return ncollection;
        }
    }
}
