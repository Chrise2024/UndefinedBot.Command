using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;

namespace Command.Raw
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
                .Description("{0}raw - 群u到底发的什么东西")
                .ShortDescription("{0}raw - 原始消息")
                .Usage("用{0}raw 回复想生成的消息")
                .Example("{0}raw")
                .Action(async (args) =>
                {
                    if (args.Param.Count > 0)
                    {
                        MsgBodySchematics targetMsg = await _undefinedApi.Api.GetMsg(args.Param[0]);
                        await _undefinedApi.Api.SendGroupMsg(
                                        args.GroupId,
                                        _undefinedApi.GetMessageBuilder()
                                            .Text(JsonConvert.SerializeObject(targetMsg.Message, Formatting.Indented)).Build()
                                    );
                    }
                    else
                    {
                        _undefinedApi.Logger.Error("raw",$"Improper Arg: Too Less args, At Command <{args.Command}>");
                    }
                });
            _undefinedApi.SubmitCommand();
        }
    }
}
