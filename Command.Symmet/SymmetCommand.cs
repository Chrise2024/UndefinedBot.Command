using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Utils;

namespace Command.Symmet
{
    public class SymmetCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        private readonly ImageConverter _imageConverter;
        public SymmetCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _imageConverter = new(_undefinedApi);
            _undefinedApi.RegisterCommand("symmet")
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
                })/*  .Then(new VariableNode("ptn1", new StringArgument())
                    .Execute(async (ctx) =>
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("图呢")
                                .Build()
                        );
                    }).Then(new VariableNode("image1",new ImageArgument())
                        .Execute(async (ctx) =>
                        {
                            string ptn = StringArgument.GetString("ptn1", ctx);
                            QImage originImage = ImageArgument.GetImage("image1", ctx);
                            string originImageUrl = originImage.Url ?? originImage.File;
                            string imageCachePath = _imageConverter.GetConvertedImage(originImageUrl, ImageContentType.Url, ptn);
                            if (imageCachePath.Length == 0)
                            {
                                _undefinedApi.Logger.Error("Pic Convert Failed");
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
                                        .Image(imageCachePath, ImageSendType.LocalFile, ImageSubType.Normal)
                                        .Build()
                                );
                                FileIo.SafeDeleteFile(imageCachePath);
                            }
                        })))
                */
                    .Then(new VariableNode("target2",new ReplyArgument())
                    .Execute(async (ctx) =>
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("不知道这条消息在哪")
                                .Build()
                        );
                    }).Then(new VariableNode("ptn2",new StringArgument())
                        .Execute(async (ctx) =>
                        {
                            string ptn = StringArgument.GetString("ptn2", ctx);
                            int msgId = ReplyArgument.GetQReply("target2", ctx).MsgId;
                            string imageCachePath = _imageConverter.GetConvertedImage($"{msgId}", ImageContentType.MsgId, ptn);
                            if (imageCachePath.Length == 0)
                            {
                                _undefinedApi.Logger.Error("Pic Convert Failed");
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
                                        .Image(imageCachePath, ImageSendType.LocalFile, ImageSubType.Normal)
                                        .Build()
                                );
                                FileIo.SafeDeleteFile(imageCachePath);
                            }
                        })));
                /*.Action(async (ctx) =>
                {
                    if (ctx.CallingProperties.Param.Count > 1)
                    {
                        string imageCachePath;
                        //ParamFormat: [Pattern] [ImageUrl]
                        if (ctx.CallingProperties.Param[1].StartsWith("http"))
                        {
                            imageCachePath = _imageConverter.GetConvertedImage(ctx.CallingProperties.Param[1], ImageContentType.Url, ctx.CallingProperties.Param[0]);
                        }
                        //ParamFormat: [MsgId] [Pattern]
                        else
                        {
                            imageCachePath = _imageConverter.GetConvertedImage(ctx.CallingProperties.Param[0], ImageContentType.MsgId, ctx.CallingProperties.Param[1]);
                        }
                        if (imageCachePath.Length == 0)
                        {
                            _undefinedApi.Logger.Error("Pic Convert Failed");
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Text("似乎转换不了").Build()
                            );
                        }
                        else
                        {
                            await ctx.Api.SendGroupMsg(
                                ctx.CallingProperties.GroupId,
                                ctx.GetMessageBuilder()
                                    .Reply(ctx.CallingProperties.MsgId)
                                    .Image(imageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                            );
                            FileIo.SafeDeleteFile(imageCachePath);
                        }
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("Improper Arg: Too Less args");
                    }
                });*/
            _undefinedApi.SubmitCommand();
        }
    }
}
