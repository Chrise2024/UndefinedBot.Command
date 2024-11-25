using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.NetWork;

namespace Command.Waifu
{
    public class WaifuCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        private readonly Dictionary<long, List<GroupMember>> _memberList = [];
        private readonly Random _randRoot = new();
        public WaifuCommand(string pluginName)
        {
            _undefinedApi = new(pluginName);
            _pluginName = pluginName;
            _undefinedApi.RegisterCommand("wife")
                .Description("群u到底发的什么东西")
                .ShortDescription("原始消息")
                .Alias(["waifu", "今日老婆"])
                .Usage("{0}wife")
                .Example("{0}wife")
                .Execute(async (ctx) =>
                {
                    if (!_memberList.TryGetValue(ctx.CallingProperties.GroupId, out List<GroupMember>? value))//(_memberList.TryGetValue(ctx.CallingProperties.GroupId,out List<GroupMember>? currentList))
                    {
                        value = await ctx.Api.GetGroupMemberList(ctx.CallingProperties.GroupId);
                        _memberList[ctx.CallingProperties.GroupId] = value;
                    }
                    List<GroupMember> currentList = value;
                    if (currentList.Count <= 1)
                    {
                        return;
                    }
                    GroupMember caller = await ctx.Api.GetGroupMember(ctx.CallingProperties.GroupId,ctx.CallingProperties.CallerUin);
                    if (caller.Nickname != null)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            GroupMember member = currentList[_randRoot.Next(currentList.Count)];
                            if (member.UserId  != caller.UserId)
                            {
                                await ctx.Api.SendGroupMsg(
                                    ctx.CallingProperties.GroupId,
                                    ctx.GetMessageBuilder()
                                        .Text($"@{(caller.Card?.Length == 0 ? caller.Nickname : caller.Card)}\n您今天的老婆群友是：\n@{(member.Card?.Length == 0 ? member.Nickname : member.Card)}")
                                        .Image($"http://q.qlogo.cn/headimg_dl?dst_uin={member.UserId}&spec=640&img_type=jpg", ImageSendType.Url, ImageSubType.Emoji)
                                        .Build()
                                );
                                return;
                            }
                        }
                    }
                });
            _undefinedApi.SubmitCommand();
        }
    }
}
