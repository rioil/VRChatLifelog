using VRChatLogWathcer.Data;
using VRChatLogWathcer.Models;

namespace VRChatLogWatcherTest
{
    public class VRChatLogUtilTest
    {
        [Theory]
        [InlineData("[Behaviour] Initialized PlayerAPI \"hoge\" is local", "hoge", true)]
        [InlineData("[Behaviour] Initialized PlayerAPI \"hoge\" is remote", "hoge", false)]
        [InlineData("[Behaviour] Initialized PlayerAPI \"fuga\" is remote", "fuga", false)]
        [InlineData("[Behaviour] Initialized PlayerAPI \"テスト\" is remote", "テスト", false)]
        public void Parse_Valid_PlayerJoinLog(string log, string playerName, bool isLocal)
        {
            var isSuccess = VRChatLogUtil.TryParsePlayerJoinLog(log, out var joinLog);
            Assert.True(isSuccess);
            Assert.NotNull(joinLog);

            Assert.Equal(playerName, joinLog!.PlayerName);
            Assert.Equal(isLocal, joinLog.IsLocal);
        }

        [Theory]
        [InlineData("")]
        [InlineData("[Behaviour] OnPlayerJoined hoge")]
        [InlineData("[Behaviour] Initialized PlayerAPI")]
        [InlineData("[Behaviour] Initialized PlayerAPI \"テスト\" is super")]
        public void Parse_Invalid_PlayerJoinLog(string log)
        {
            var isSuccess = VRChatLogUtil.TryParsePlayerJoinLog(log, out var joinLog);
            Assert.False(isSuccess);
            Assert.Null(joinLog);
        }

        [Theory]
        [InlineData("[Behaviour] Unregistering hoge", "hoge")]
        [InlineData("[Behaviour] Unregistering テスト用", "テスト用")]
        public void Parse_Valid_PlayerLeftLog(string log, string playerName)
        {
            var isSuccess = VRChatLogUtil.TryParsePlayerLeftLog(log, out var leftLog);
            Assert.True(isSuccess);
            Assert.NotNull(leftLog);

            Assert.Equal(playerName, leftLog!.PlayerName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("[Behaviour] Unregistering")]
        [InlineData("[Behaviour] Initialized PlayerAPI")]
        public void Parse_Invalid_PlayerLeftLog(string log)
        {
            var isSuccess = VRChatLogUtil.TryParsePlayerLeftLog(log, out var leftLog);
            Assert.False(isSuccess);
            Assert.Null(leftLog);
        }

        [Theory]
        [InlineData(
            "[Behaviour] Joining wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:11543~friends(usr_01234567-abcd-1234-56de-0123456789ef)~region(jp)~nonce(2413efc9-5f8c-4fd5-8a89-e6f54eb1bf32)",
            "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd",
            "11543",
            EInstanceType.Friends,
            ERegion.JP,
            "usr_01234567-abcd-1234-56de-0123456789ef")]
        [InlineData(
            "[Behaviour] Joining wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:123~private(usr_01234567-abcd-1234-56de-0123456789ef)~canRequestInvite~region(eu)~nonce(2413efc9-5f8c-4fd5-8a89-e6f54eb1bf32)",
            "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd",
            "123",
            EInstanceType.InvitePlus,
            ERegion.EU,
            "usr_01234567-abcd-1234-56de-0123456789ef")]
        [InlineData(
            "[Behaviour] Joining wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:73158~region(use)",
            "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd",
            "73158",
            EInstanceType.Public,
            ERegion.USE,
            "")]
        [InlineData(
            "[Behaviour] Joining wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:62305~hidden(feDc31J9Az)~region(us)~nonce(cc29b63f-e488-4802-a28d-d2b53459d000))",
            "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd",
            "62305",
            EInstanceType.FriendsPlus,
            ERegion.USW,
            "feDc31J9Az")]
        [InlineData(
            "[Behaviour] Joining wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:3456~group(grp_1ef362ad-f525-4ab8-900b-0071e6b9610a)~groupAccessType(members)~region(jp)",
            "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd",
            "3456",
            EInstanceType.Group,
            ERegion.JP,
            "grp_1ef362ad-f525-4ab8-900b-0071e6b9610a")]
        [InlineData(
            "[Behaviour] Joining wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:9072~group(grp_1ef362ad-f525-4ab8-900b-0071e6b9610a)~groupAccessType(plus)~region(jp)",
            "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd",
            "9072",
            EInstanceType.GroupPlus,
            ERegion.JP,
            "grp_1ef362ad-f525-4ab8-900b-0071e6b9610a")]
        [InlineData(
            "[Behaviour] Joining wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:9072~group(grp_1ef362ad-f525-4ab8-900b-0071e6b9610a)~groupAccessType(public)~region(jp)",
            "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd",
            "9072",
            EInstanceType.GroupPublic,
            ERegion.JP,
            "grp_1ef362ad-f525-4ab8-900b-0071e6b9610a")]
        public void Parse_Valid_WorldJoinLog(string log, string worldId, string instanceId, EInstanceType type, ERegion region, string masterId)
        {
            var isSuccess = VRChatLogUtil.TryParseWorldJoinLog(log, out var instance);
            Assert.True(isSuccess);
            Assert.NotNull(instance);

            Assert.Equal(worldId, instance!.WorldId);
            Assert.Equal(instanceId, instance.InstanceId);
            Assert.Equal(type, instance.Type);
            Assert.Equal(region, instance.Region);
            Assert.Equal(masterId, instance.MasterId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("[Behaviour] Joining wrld_4432zz9x-729c-46e3-8eaf-846aa0a37fdd:11543~friends(usr_01234567-abcd-1234-56de-0123456789ef)~region(jp)~nonce(2413efc9-5f8c-4fd5-8a89-e6f54eb1bf32)")]
        [InlineData("[Behaviour] Initialized PlayerAPI \"fuga\" is remote")]
        public void Parse_Invalid_WorldJoinLog(string log)
        {
            var isSuccess = VRChatLogUtil.TryParseWorldJoinLog(log, out var instance);
            Assert.False(isSuccess);
            Assert.Null(instance);
        }

        [Theory]
        [InlineData("[Behaviour] Joining or Creating Room: sample world", "sample world")]
        [InlineData("[Behaviour] Joining or Creating Room: テスト用ワールド", "テスト用ワールド")]
        public void Parse_Valid_RoomJoinLog(string log, string worldName)
        {
            var isSuccess = VRChatLogUtil.TryParseRoomJoinLog(log, out var room);
            Assert.True(isSuccess);
            Assert.NotNull(room);

            Assert.Equal(worldName, room!.WorldName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("[Behaviour] Joining or Creating World: sample world")]
        [InlineData("[Behaviour] Joining wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:11543~friends(usr_01234567-abcd-1234-56de-0123456789ef)~region(jp)~nonce(2413efc9-5f8c-4fd5-8a89-e6f54eb1bf32)")]
        public void Parse_Invalid_RoomJoinLog(string log)
        {
            var isSuccess = VRChatLogUtil.TryParseRoomJoinLog(log, out var room);
            Assert.False(isSuccess);
            Assert.Null(room);
        }
    }
}
