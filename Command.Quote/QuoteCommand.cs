using System.Drawing.Imaging;
using System.Drawing;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace Command.Quote
{
    public class QuoteCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        private readonly HttpClient _httpClient = new();
        public QuoteCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("quote")
                .Alias(["q"])
                .Description("正义史官 - 生成切片（入典）")
                .ShortDescription("生成切片（入典）")
                .Usage("用{0}quote 回复想生成的消息")
                .Example("{0}quote")
                .Action(async (args) =>
                {
                    if (args.Param.Count > 0)
                    {
                        string imageCachePath = GenquoteImage(args.Param[0]);
                        if (imageCachePath.Length == 0)
                        {
                            _undefinedApi.Logger.Error("quote","Generate Failed");
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
                                        .Image(imageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                                );
                            FileIO.SafeDeleteFile(imageCachePath);
                        }
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("quote", "Improper Arg: Too Less args");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
        private string GenquoteImage(string targetMsgIdString)
        {
            MsgBodySchematics targetMsg = _undefinedApi.Api.GetMsg(Int32.TryParse(targetMsgIdString, out int targetMsgId) ? targetMsgId : 0).Result;
            if ((targetMsg.MessageId ?? 0) == 0)
            {
                return "";
            }
            else
            {
                string targetMsgString = "";
                List<JObject> msgSeq = targetMsg.Message ?? [];
                foreach (JObject index in msgSeq)
                {
                    if (index.Value<string>("type")?.Equals("text") ?? false)
                    {
                        string text = index.Value<JObject>("data")?.Value<string>("text") ?? "";
                        if (text.Length != 0 && !RegexProvider.GetEmptyStringRegex().IsMatch(text))
                        {
                            targetMsgString += text;
                        }
                    }
                    else if (index.Value<string>("type")?.Equals("at") ?? false)
                    {
                        targetMsgString += (index.Value<JObject>("data")?.Value<string>("name") ?? "@") + " ";
                    }
                    else if (index.Value<string>("type")?.Equals("face") ?? false)
                    {
                        string fId = index.Value<JObject>("data")?.Value<string>("id") ?? "";
                        targetMsgString += (TextRender.QFaceReference.GetValueOrDefault(fId, ""));
                    }
                }
                long targetUin = targetMsg.Sender?.UserId ?? 0;
                GroupMemberSchematics cMember = _undefinedApi.Api.GetGroupMember(targetMsg.GroupId ?? 0, targetUin).Result;
                string targetName = $"@{cMember.Nickname ?? ""}";
                string imCachePath = Path.Join(_undefinedApi.CachePath, $"{DateTime.Now:HH-mm-ss}.png");
                string qSplashPath = Path.Join(_undefinedApi.PluginPath, "QSplash.png");
                string qTextCachePath = Path.Join(_undefinedApi.CachePath, $"Text-{DateTime.Now:HH-mm-ss}.png");
                string qnnCachePath = Path.Join(_undefinedApi.CachePath, $"NickName-{DateTime.Now:HH-mm-ss}.png");
                if (File.Exists(qSplashPath))
                {
                    //min: 108 max: 252 mid: 165
                    //108-160-240
                    Image coverImage = Image.FromFile(qSplashPath);
                    Image targetAvatar = GetQqAvatar(targetUin).Result;
                    Bitmap bg = new(1200, 640);
                    Graphics g = Graphics.FromImage(bg);
                    g.DrawImage(targetAvatar, 0, 0, 640, 640);
                    g.DrawImage(coverImage, 0, 0, 1200, 640);
                    TextRender.GenTextImage(qTextCachePath, targetMsgString, 96, 1800, 1350);
                    TextRender.GenTextImage(qnnCachePath, targetName, 72, 1500, 120);
                    Bitmap textBmp = new(qTextCachePath);
                    Bitmap nameBmp = new(qnnCachePath);
                    g.DrawImage(textBmp, 550, 95, 600, 450);
                    g.DrawImage(nameBmp, 600, 600, 500, 40);
                    //g.DrawString(TargetMsgString, new Font("Noto Color Emoji", 40, FontStyle.Regular), new SolidBrush(Color.White), new RectangleF(440, 170, 800, 300), format);
                    //g.DrawString(TargetName, new Font("Noto Color Emoji", 24, FontStyle.Regular), new SolidBrush(Color.White), new RectangleF(690, 540, 300, 80), format);
                    bg.Save(imCachePath, ImageFormat.Png);
                    textBmp.Dispose();
                    nameBmp.Dispose();
                    g.Dispose();
                    bg.Dispose();
                    coverImage.Dispose();
                    targetAvatar.Dispose();
                    FileIO.SafeDeleteFile(qnnCachePath);
                    FileIO.SafeDeleteFile(qTextCachePath);
                    return imCachePath;
                }
                else
                {
                    return "";
                }
            }
        }
        private async Task<Image> GetQqAvatar<T>(T targetUin)
        {
            try
            {
                byte[] imageBytes = await _httpClient.GetByteArrayAsync($"http://q.qlogo.cn/headimg_dl?dst_uin={targetUin}&spec=640&img_type=jpg");
                if (imageBytes.Length > 0)
                {
                    MemoryStream ms = new MemoryStream(imageBytes);
                    Image im = Image.FromStream(ms);
                    ms.Close();
                    return im;
                }
                else
                {
                    return new Bitmap(1, 1);
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Canceled: ");
                _undefinedApi.Logger.Error("Genquote",ex.Message);
                _undefinedApi.Logger.Error("Genquote", ex.StackTrace ?? "");
                return new Bitmap(1, 1);
            }
            catch
            {
                return new Bitmap(1, 1);
            }
        }
    }
}
