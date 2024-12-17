using System.Drawing.Imaging;
using System.Drawing;
using System.Text.Json.Nodes;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Registry;

namespace Command.Quote;

public class QuoteCommand : IPluginInitializer
{
    private readonly HttpClient _httpClient = new();

    public void Initialize(UndefinedApi undefinedApi)
    {
        undefinedApi.RegisterCommand("quote")
            .Alias(["q", "入典"])
            .Description("正义史官 - 生成切片（入典）")
            .ShortDescription("生成切片（入典）")
            .Usage("用{0}quote 回复想生成的消息")
            .Example("{0}quote")
            .Execute(async (ctx) =>
            {
                await ctx.Api.SendGroupMsg(
                    ctx.CallingProperties.GroupId,
                    ctx.GetMessageBuilder()
                        .Text("不知道这条消息在哪")
                        .Build()
                );
            }).Then(new VariableNode("target", new ReplyArgument())
                .Execute(async (ctx) =>
                {
                    string imageCachePath = GenQuoteImage(ReplyArgument.GetQReply("target", ctx).MsgId,undefinedApi);
                    if (imageCachePath.Length == 0)
                    {
                        undefinedApi.Logger.Error("Generate Failed");
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("生成出错了").Build()
                        );
                    }
                    else
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Image(imageCachePath).Build()
                        );
                        //FileIO.SafeDeleteFile(imageCachePath);
                    }
                }));
    }

    private string GenQuoteImage(int targetMsgId,UndefinedApi undefinedApi)
    {
        MsgBody? targetMsg = undefinedApi.Api.GetMsg(targetMsgId).Result;
        if (targetMsg == null || targetMsg.MessageId == 0)
        {
            return "";
        }

        long targetUin = targetMsg.Sender.UserId;
        GroupMember? cMember = undefinedApi.Api.GetGroupMember(targetMsg.GroupId, targetUin).Result;
        string qSplashPath = Path.Join(undefinedApi.PluginPath, "QSplash.png");
        string targetName = $"@{cMember?.Nickname ?? ""}";
        string imCacheName = $"MsgId{targetMsg.MessageId}Quote.png";
        string qTextCacheName = $"MsgId{targetMsg.MessageId}Text.png";
        string qnnCacheName = $"NickName{targetUin}Text.png";
        string targetMsgString = "";
        List<JsonNode> msgSeq = targetMsg.Message;
        string imCachePath = undefinedApi.Cache.GetFile(imCacheName);
        if (imCachePath.Length != 0)
        {
            return imCachePath;
        }
        else
        {
            imCachePath = undefinedApi.Cache.AddFile(imCacheName, imCacheName, 60 * 60 * 24);
        }

        foreach (JsonNode index in msgSeq)
        {
            if (index["type"]?.GetValue<string>().Equals("text") ?? false)
            {
                string text = index["data"]?.GetValue<JsonNode>()["text"]?.GetValue<string>() ?? "";
                if (text.Length != 0 && !RegexProvider.GetEmptyStringRegex().IsMatch(text))
                {
                    targetMsgString += text;
                }
            }
            else if (index["type"]?.GetValue<string>().Equals("at") ?? false)
            {
                targetMsgString += (index["data"]?.GetValue<JsonNode>()["name"]?.GetValue<string>() ?? "@") + " ";
            }
            else if (index["type"]?.GetValue<string>().Equals("face") ?? false)
            {
                string fId = index["data"]?.GetValue<JsonNode>()["id"]?.GetValue<string>() ?? "";
                targetMsgString += (TextRender.QFaceReference.GetValueOrDefault(fId, ""));
            }
        }

        if (File.Exists(qSplashPath))
        {
            //min: 108 max: 252 mid: 165
            //108-160-240
            Image coverImage = Image.FromFile(qSplashPath);
            Image targetAvatar = GetQQAvatar(targetUin,undefinedApi).Result;
            Bitmap bg = new(1200, 640);
            Graphics g = Graphics.FromImage(bg);
            g.DrawImage(targetAvatar, 0, 0, 640, 640);
            g.DrawImage(coverImage, 0, 0, 1200, 640);
            string qTextCachePath = undefinedApi.Cache.GetFile(qTextCacheName);
            if (qTextCachePath.Length == 0)
            {
                qTextCachePath = undefinedApi.Cache.AddFile(qTextCacheName, qTextCacheName, 60 * 60 * 24);
                TextRender.GenTextImage(qTextCachePath, targetMsgString, 96, 1800, 1350);
            }

            string qnnCachePath = undefinedApi.Cache.GetFile(qnnCacheName);
            if (qnnCachePath.Length == 0)
            {
                qnnCachePath = undefinedApi.Cache.AddFile(qnnCacheName, qnnCacheName, 60 * 60 * 24);
                TextRender.GenTextImage(qnnCachePath, targetName, 72, 1500, 120);
            }

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
            //FileIO.SafeDeleteFile(qnnCachePath);
            //FileIO.SafeDeleteFile(qTextCachePath);
            return imCachePath;
        }
        else
        {
            return "";
        }
    }

    private async Task<Image> GetQQAvatar<T>(T targetUin,UndefinedApi undefinedApi)
    {
        try
        {
            byte[] imageBytes =
                await _httpClient.GetByteArrayAsync(
                    $"http://q.qlogo.cn/headimg_dl?dst_uin={targetUin}&spec=640&img_type=jpg");
            if (imageBytes.Length > 0)
            {
                MemoryStream ms = new(imageBytes);
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
            undefinedApi.Logger.Error(ex.Message);
            undefinedApi.Logger.Error(ex.StackTrace ?? "");
            return new Bitmap(1, 1);
        }
        catch
        {
            return new Bitmap(1, 1);
        }
    }
}
