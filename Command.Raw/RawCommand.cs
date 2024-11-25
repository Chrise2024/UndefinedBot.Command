using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

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
                        MsgBody targetMsg = await ctx.Api.GetMsg(ReplyArgument.GetQReply("target",ctx).MsgId);
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text(JsonConvert.SerializeObject(targetMsg.Message, Formatting.Indented)).Build()
                        );
                    }));
            _undefinedApi.SubmitCommand();
        }
    }
}
