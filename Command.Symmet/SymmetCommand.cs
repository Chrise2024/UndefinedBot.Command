using UndefinedBot.Core;
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
                .Usage("{0}symmet [对称方法] [表情/图片] Or 用 {0}symmet [对称方法] 回复[表情/图片]")
                .Example("{0}symmet 上下 [图片]")
                .Action(async (commandContext) =>
                {
                    if (commandContext.Args.Param.Count > 1)
                    {
                        string imageCachePath;
                        //ParamFormat: [Pattern] [ImageUrl]
                        if (commandContext.Args.Param[1].StartsWith("http"))
                        {
                            imageCachePath = _imageConverter.GetConvertedImage(commandContext.Args.Param[1], ImageContentType.Url, commandContext.Args.Param[0]);
                        }
                        //ParamFormat: [MsgId] [Pattern]
                        else
                        {
                            imageCachePath = _imageConverter.GetConvertedImage(commandContext.Args.Param[0], ImageContentType.MsgId, commandContext.Args.Param[1]);
                        }
                        if (imageCachePath.Length == 0)
                        {
                            _undefinedApi.Logger.Error("Pic Convert Failed");
                            await commandContext.Api.SendGroupMsg(
                                commandContext.Args.GroupId,
                                commandContext.GetMessageBuilder()
                                    .Text("似乎转换不了").Build()
                            );
                        }
                        else
                        {
                            await commandContext.Api.SendGroupMsg(
                                commandContext.Args.GroupId,
                                commandContext.GetMessageBuilder()
                                    .Reply(commandContext.Args.MsgId)
                                    .Image(imageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                            );
                            FileIo.SafeDeleteFile(imageCachePath);
                        }
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("Improper Arg: Too Less args");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
    }
}
