using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using Newtonsoft.Json;

namespace Command.Template
{
    public class RawCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        public RawCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("raw")
                .Description("{0}raw - 群u到底发的什么东西\n使用方法：用{0}raw 回复想生成的消息")
                .ShortDescription("{0}raw - 原始消息")
                .Example("{0}raw")
                .Action(async (ArgSchematics args) =>
                {
                    if (args.Param.Count > 0)
                    {
                        MsgBodySchematics TargetMsg = await _undefinedApi.Api.GetMsg(args.Param[0]);
                        await _undefinedApi.Api.SendGroupMsg(
                                        args.GroupId,
                                        _undefinedApi.GetMessageBuilder()
                                            .Text(JsonConvert.SerializeObject(TargetMsg.Message, Formatting.Indented)).Build()
                                    );
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("raw",$"Unproper Arg: Too Less args, At Command <{args.Command}>");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
    }
}
