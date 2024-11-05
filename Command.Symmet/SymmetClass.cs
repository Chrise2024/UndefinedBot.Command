using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Net.Extra;

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
                .Description("{0}symmet - 图片、表情对称\n使用方法：{0}symmet <对称方法> [表情/图片] 或用 {0}symmet <对称方法> 回复[表情/图片]，支持上下、下上、左右、右左")
                .ShortDescription("{0}symmet - 图片、表情对称")
                .Example("{0}symmet 上下 [图片]")
                .Action(async (ArgSchematics args) =>
                {
                    if (args.Param.Count > 1)
                    {
                        string ImageCachePath;
                        //ParamFormat: [Pattern] [ImageUrl]
                        if (args.Param[1].StartsWith("http"))
                        {
                            ImageCachePath = _imageConverter.GetConvertedImage(args.Param[1], ImageContentType.Url, args.Param[0]);
                        }
                        //ParamFormat: [MsgId] [Pattern]
                        else
                        {
                            ImageCachePath = _imageConverter.GetConvertedImage(args.Param[0], ImageContentType.MsgId, args.Param[1]);
                        }
                        if (ImageCachePath.Length == 0)
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
                                            .Image(ImageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                                    );
                            FileIO.SafeDeleteFile(ImageCachePath);
                        }
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("symmet","Unproper Arg: Too Less args");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
    }
}
