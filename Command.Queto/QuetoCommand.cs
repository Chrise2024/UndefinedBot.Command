using System.Drawing.Imaging;
using System.Drawing;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using SkiaSharp;

namespace Command.Queto
{
    public class QuetoCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        private readonly HttpClient _httpClient = new();
        public QuetoCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("queto")
                .Alias(["q"])
                .Description("{0}queto - 生成切片（入典）\n使用方法：用{0}queto 回复想生成的消息")
                .ShortDescription("{0}queto - 生成切片（入典）")
                .Example("{0}queto")
                .Action(async (ArgSchematics args) =>
                {
                    if (args.Param.Count > 0)
                    {
                        string ImageCachePath = GenQuetoImage(args.Param[0]);
                        if (ImageCachePath.Length == 0)
                        {
                            _undefinedApi.Logger.Error("queto","Generate Failed");
                            await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text("生成出错了").Build()
                            );
                        }
                        else
                        {
                            await _undefinedApi.Api.SendGroupMsg(
                                    args.GroupId,
                                    _undefinedApi.GetMessageBuilder()
                                        .Image(ImageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                                );
                            FileIO.SafeDeleteFile(ImageCachePath);
                        }
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("queto", "Unproper Arg: Too Less args");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
        private string GenQuetoImage(string targetMsgIdString)
        {
            MsgBodySchematics TargetMsg = _undefinedApi.Api.GetMsg(Int32.TryParse(targetMsgIdString, out int TargetMsgId) ? TargetMsgId : 0).Result;
            if ((TargetMsg.MessageId ?? 0) == 0)
            {
                return "";
            }
            else
            {
                string TargetMsgString = "";
                List<JObject> MsgSeq = TargetMsg.Message ?? [];
                foreach (JObject index in MsgSeq)
                {
                    if (index.Value<string>("type")?.Equals("text") ?? false)
                    {
                        string TText = index.Value<JObject>("data")?.Value<string>("text") ?? "";
                        if (TText.Length != 0 && !RegexProvider.GetEmptyStringRegex().IsMatch(TText))
                        {
                            TargetMsgString += TText;
                        }
                    }
                    else if (index.Value<string>("type")?.Equals("at") ?? false)
                    {
                        TargetMsgString += (index.Value<JObject>("data")?.Value<string>("name") ?? "@") + " ";
                    }
                    else if (index.Value<string>("type")?.Equals("face") ?? false)
                    {
                        string FId = index.Value<JObject>("data")?.Value<string>("id") ?? "";
                        TargetMsgString += (TextRender.QFaceReference.TryGetValue(FId, out var EmojiString) ? EmojiString : "");
                    }
                    else
                    {
                        continue;
                    }
                }
                long TargetUin = TargetMsg.Sender?.UserId ?? 0;
                GroupMemberSchematics CMember = _undefinedApi.Api.GetGroupMember(TargetMsg.GroupId ?? 0, TargetUin).Result;
                string TargetName = $"@{CMember.Nickname ?? ""}";
                string ImCachePath = Path.Join(_undefinedApi.CachePath, $"{DateTime.Now:HH-mm-ss}.png");
                string QSplashPath = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "QSplash.png");
                string QTextCachePath = Path.Join(_undefinedApi.CachePath, $"Text-{DateTime.Now:HH-mm-ss}.png");
                string QNNCachePath = Path.Join(_undefinedApi.CachePath, $"NickName-{DateTime.Now:HH-mm-ss}.png");
                if (File.Exists(QSplashPath))
                {
                    //min: 108 max: 252 mid: 165
                    //108-160-240
                    Image CoverImage = Image.FromFile(QSplashPath);
                    Image TargetAvatar = GetQQAvatar(TargetUin).Result;
                    Bitmap bg = new(1200, 640);
                    Graphics g = Graphics.FromImage(bg);
                    g.DrawImage(TargetAvatar, 0, 0, 640, 640);
                    g.DrawImage(CoverImage, 0, 0, 1200, 640);
                    TextRender.GenTextImage(QTextCachePath, TargetMsgString, 96, 1800, 1350);
                    TextRender.GenTextImage(QNNCachePath, TargetName, 72, 1500, 120);
                    Bitmap TextBmp = new(QTextCachePath);
                    Bitmap NameBmp = new(QNNCachePath);
                    g.DrawImage(TextBmp, 550, 95, 600, 450);
                    g.DrawImage(NameBmp, 600, 600, 500, 40);
                    //g.DrawString(TargetMsgString, new Font("Noto Color Emoji", 40, FontStyle.Regular), new SolidBrush(Color.White), new RectangleF(440, 170, 800, 300), format);
                    //g.DrawString(TargetName, new Font("Noto Color Emoji", 24, FontStyle.Regular), new SolidBrush(Color.White), new RectangleF(690, 540, 300, 80), format);
                    bg.Save(ImCachePath, ImageFormat.Png);
                    TextBmp.Dispose();
                    NameBmp.Dispose();
                    g.Dispose();
                    bg.Dispose();
                    CoverImage.Dispose();
                    TargetAvatar.Dispose();
                    FileIO.SafeDeleteFile(QNNCachePath);
                    FileIO.SafeDeleteFile(QTextCachePath);
                    return ImCachePath;
                }
                else
                {
                    return "";
                }
            }
        }
        private async Task<Image> GetQQAvatar<T>(T targetUin)
        {
            try
            {
                byte[] ImageBytes = await _httpClient.GetByteArrayAsync($"http://q.qlogo.cn/headimg_dl?dst_uin={targetUin}&spec=640&img_type=jpg");
                if (ImageBytes.Length > 0)
                {
                    MemoryStream ms = new MemoryStream(ImageBytes);
                    Image Im = Image.FromStream(ms);
                    ms.Close();
                    return Im;
                }
                else
                {
                    return new Bitmap(1, 1);
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _undefinedApi.Logger.Error("GenQueto",ex.Message);
                _undefinedApi.Logger.Error("GenQueto", ex.StackTrace ?? "");
                return new Bitmap(1, 1);
            }
            catch
            {
                return new Bitmap(1, 1);
            }
        }
    }
}
