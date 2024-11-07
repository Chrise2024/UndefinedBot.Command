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
                .Description("图片、表情对称\n支持上下、下上、左右、右左、左上、左下、右上、右下")
                .ShortDescription("图片、表情对称")
                .Usage("{0}symmet [对称方法] [表情/图片] Or 用 {0}symmet [对称方法] 回复[表情/图片]")
                .Example("{0}symmet 上下 [图片]")
                .Action(async (args) =>
                {
                    if (args.Param.Count > 1)
                    {
                        string imageCachePath;
                        //ParamFormat: [Pattern] [ImageUrl]
                        if (args.Param[1].StartsWith("http"))
                        {
                            imageCachePath = _imageConverter.GetConvertedImage(args.Param[1], ImageContentType.Url, args.Param[0]);
                        }
                        //ParamFormat: [MsgId] [Pattern]
                        else
                        {
                            imageCachePath = _imageConverter.GetConvertedImage(args.Param[0], ImageContentType.MsgId, args.Param[1]);
                        }
                        if (imageCachePath.Length == 0)
                        {
                            _undefinedApi.Logger.Error("symmet", "Pic Convert Failed");
                            await _undefinedApi.Api.SendGroupMsg(
                                args.GroupId,
                                _undefinedApi.GetMessageBuilder()
                                    .Text("似乎转换不了").Build()
                            );
                        }
                        else
                        {
                            await _undefinedApi.Api.SendGroupMsg(
                                        args.GroupId,
                                        _undefinedApi.GetMessageBuilder()
                                            .Reply(args.MsgId)
                                            .Image(imageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                                    );
                            FileIo.SafeDeleteFile(imageCachePath);
                        }
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("symmet","Improper Arg: Too Less args");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
    }
}
