using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.NetWork;

namespace Command.Waifu
{
    public class WaifuCommand
    {
        private readonly UndefinedAPI _undefinedApi;
        private readonly string _pluginName;
        private readonly Dictionary<long, GroupMemberList> _memberList = [];
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
                    if (!_memberList.TryGetValue(ctx.CallingProperties.GroupId, out GroupMemberList? currentList))//(_memberList.TryGetValue(ctx.CallingProperties.GroupId,out List<GroupMember>? currentList))
                    {
                        currentList = await GroupMemberList.Constructor(ctx);
                        _memberList[ctx.CallingProperties.GroupId] = currentList;
                    }
                    GroupMember caller = await ctx.Api.GetGroupMember(ctx.CallingProperties.GroupId,ctx.CallingProperties.CallerUin);
                    GroupMember member = await currentList.Roll(ctx);
                    if (member.UserId == null)
                    {
                        await ctx.Api.SendGroupMsg(
                            ctx.CallingProperties.GroupId,
                            ctx.GetMessageBuilder()
                                .Text("获取失败")
                                .Build()
                            );
                        return;
                    }
                    await ctx.Api.SendGroupMsg(
                        ctx.CallingProperties.GroupId,
                        ctx.GetMessageBuilder()
                            .Text($"@{(caller.Card?.Length == 0 ? caller.Nickname : caller.Card)}\n您今天的老婆群友是：\n@{(member.Card?.Length == 0 ? member.Nickname : member.Card)}")
                            .Image($"http://q.qlogo.cn/headimg_dl?dst_uin={member.UserId}&spec=640&img_type=jpg", ImageSendType.Url, ImageSubType.Emoji)
                            .Build()
                    );
                });
            _undefinedApi.SubmitCommand();
        }
    }

    internal class GroupMemberList
    {
        private List<GroupMember> _memberList;
        private int _counter = 0;
        private DateTime _lastRoll = DateTime.Now;
        private readonly Random _randomRoot = new();
        private GroupMemberList(List<GroupMember> memberList)
        {
            _memberList = memberList;
        }

        public static async Task<GroupMemberList> Constructor(CommandContext ctx)
        {
            return new GroupMemberList(await ctx.Api.GetGroupMemberList(ctx.CallingProperties.GroupId));
        }

        public async Task<GroupMember> Roll(CommandContext ctx)
        {
            if (_memberList.Count == 0 || _counter++ == 5 || (DateTime.Now - _lastRoll).TotalSeconds > 86400)
            {
                _memberList = await ctx.Api.GetGroupMemberList(ctx.CallingProperties.GroupId);
                _counter = 0;
                _lastRoll = DateTime.Now;
            }
            return _memberList.Count > 1 ? _memberList.Where(item => item.UserId != ctx.CallingProperties.CallerUin).ToArray()[
                _randomRoot.Next(_memberList.Count - 2)] : default;
        }
    }
}
