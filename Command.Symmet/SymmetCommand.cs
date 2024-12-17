using UndefinedBot.Core;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Registry;

namespace Command.Symmet
{
    public class SymmetCommand : IPluginInitializer
    {
        private ImageConverter? ImageConverter { get; set; }

        public void Initialize(UndefinedApi undefinedApi)
        {
            ImageConverter = new(undefinedApi);
            undefinedApi.RegisterCommand("symmet")
                .Alias(["对称"])
                .Description("图片、表情对称\n支持上下、下上、左右、右左、左上、左下、右上、右下")
                .ShortDescription("图片、表情对称")
                .Usage("用 {0}symmet [对称方法] 回复[表情/图片]")
                //.Usage("{0}symmet [对称方法] [表情/图片] Or 用 {0}symmet [对称方法] 回复[表情/图片]")
                .Example("{0}symmet 上下 [图片]")
                .Execute(async (ctx) =>
                {
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text("")
                            .Build()
                    );
                }).Then(new VariableNode("ptn1", new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("图呢")
                                .Build()
                        );
                    }).Then(new VariableNode("image1", new ImageArgument())
                        .Execute(async (ctx) =>
                        {
                            string ptn = StringArgument.GetString("ptn1", ctx);
                            QImage originImage = ImageArgument.GetImage("image1", ctx);
                            string originImageUrl = originImage.Url ?? originImage.File;
                            string imageCachePath =
                                ImageConverter.GetConvertedImage(originImageUrl, ImageContentType.Url, ptn);
                            if (imageCachePath.Length == 0)
                            {
                                undefinedApi.Logger.Error("Pic Convert Failed");
                                await ctx.Api.SendGroupMsg(
                                    ctx.CallingProperties.GroupId,
                                    ctx.GetMessageBuilder()
                                        .Text("似乎转换不了")
                                        .Build()
                                );
                            }
                            else
                            {
                                await ctx.Api.SendGroupMsg(
                                    ctx.CallingProperties.GroupId,
                                    ctx.GetMessageBuilder()
                                        .Reply(ctx.CallingProperties.MsgId)
                                        .Image(imageCachePath)
                                        .Build()
                                );
                                File.Delete(imageCachePath);
                            }
                        })))
                .Then(new VariableNode("target2", new ReplyArgument())
                    .Execute(async (ctx) =>
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("不知道这条消息在哪")
                                .Build()
                        );
                    }).Then(new VariableNode("ptn2", new StringArgument())
                        .Execute(async (ctx) =>
                        {
                            string ptn = StringArgument.GetString("ptn2", ctx);
                            int msgId = ReplyArgument.GetQReply("target2", ctx).MsgId;
                            string imageCachePath =
                                ImageConverter.GetConvertedImage($"{msgId}", ImageContentType.MsgId, ptn);
                            if (imageCachePath.Length == 0)
                            {
                                undefinedApi.Logger.Error("Pic Convert Failed");
                                await ctx.Api.SendGroupMsg(
                                    ctx.CallingProperties.GroupId,
                                    ctx.GetMessageBuilder()
                                        .Text("似乎转换不了")
                                        .Build()
                                );
                            }
                            else
                            {
                                await ctx.Api.SendGroupMsg(
                                    ctx.CallingProperties.GroupId,
                                    ctx.GetMessageBuilder()
                                        .Reply(ctx.CallingProperties.MsgId)
                                        .Image(imageCachePath)
                                        .Build()
                                );
                                File.Delete(imageCachePath);
                            }
                        })
                    )
                );
        }
    }
}
