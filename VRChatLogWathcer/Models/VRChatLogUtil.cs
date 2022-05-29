using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace VRChatLogWathcer.Models;

public static class VRChatLogUtil
{
    private static readonly Regex PlayerJoinLogPattern = new("\\[Behaviour\\] Initialized PlayerAPI \"(?<player>.*)\" is (?<type>(remote)|(local))");

    private static readonly Regex PlayerLeftLogPattern = new(@"\[Behaviour\] Unregistering (?<player>.*)");

    private static readonly Regex WorldJoinLogPattern = new(@"\[Behaviour\] Joining (?<worldId>wr?ld_[a-z\d]{8}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{12}):(?<instanceId>\w+)(~(?<type>[\w]+)\((?<master>(usr_[a-z\d]{8}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{12})|\w{10})\))?(?<canReqInvite>\~canRequestInvite)?(~region\((?<region>[\w]+)\))?(~nonce\((.+)\))?");

    private static readonly Regex RoomJoinLogPattern = new(@"\[Behaviour\] Joining or Creating Room: (?<name>.*)");

    /// <summary>
    /// プレイヤーJoinログを解析します．
    /// </summary>
    /// <param name="log">ログ文字列</param>
    /// <param name="joinLog">解析したプレイヤーJoinログ</param>
    /// <returns>解析に成功すればtrue，解析に失敗すればfalse</returns>
    public static bool TryParsePlayerJoinLog(string log, [NotNullWhen(true)] out PlayerJoinLog? joinLog)
    {
        var match = PlayerJoinLogPattern.Match(log);
        if (!match.Success)
        {
            joinLog = null;
            return false;
        }

        var playerName = match.Groups["player"].Value;
        var isLocal = match.Groups["type"].Value == "local";

        joinLog = new PlayerJoinLog(playerName, isLocal);
        return true;
    }

    /// <summary>
    /// プレイヤーLeftログを解析します．
    /// </summary>
    /// <param name="log">ログ文字列</param>
    /// <param name="leftLog">解析したプレイヤーLeftログ</param>
    /// <returns>解析に成功すればtrue，解析に失敗すればfalse</returns>
    public static bool TryParsePlayerLeftLog(string log, [NotNullWhen(true)] out PlayerLeftLog? leftLog)
    {
        var match = PlayerLeftLogPattern.Match(log);
        if (!match.Success)
        {
            leftLog = null;
            return false;
        }

        var playerName = match.Groups["player"].Value;

        leftLog = new PlayerLeftLog(playerName);
        return true;
    }

    /// <summary>
    /// ワールドJoinログを解析します．
    /// </summary>
    /// <param name="log">ログ文字列</param>
    /// <param name="instance">Join先インスタンス情報</param>
    /// <returns>解析に成功すればtrue，解析に失敗すればfalse</returns>
    public static bool TryParseWorldJoinLog(string log, [NotNullWhen(true)] out Instance? instance)
    {
        var match = WorldJoinLogPattern.Match(log);
        if (!match.Success)
        {
            instance = null;
            return false;
        }

        var worldId = match.Groups["worldId"].Value;
        var instanceId = match.Groups["instanceId"].Value;

        var instanceType = match.Groups["type"].Value;
        var canReqInvite = !string.IsNullOrEmpty(match.Groups["canReqInvite"].Value);
        var eInstanceType = instanceType switch
        {
            "hidden" => EInstanceType.FriendsPlus,
            "friends" => EInstanceType.Friends,
            "private" => canReqInvite ? EInstanceType.InvitePlus : EInstanceType.Invite,
            "" => EInstanceType.Public,
            _ => EInstanceType.Unknown,
        };

        if (!Enum.TryParse<ERegion>(match.Groups["region"].Value, true, out var eRegion))
        {
            eRegion = ERegion.Unknown;
        }

        var master = match.Groups["master"].Value;

        instance = new Instance(worldId, instanceId, eInstanceType, eRegion, master);
        return true;
    }

    /// <summary>
    /// ルームJoinログを解析します．
    /// </summary>
    /// <param name="log">ログ文字列</param>
    /// <param name="room">ルームJoinログ</param>
    /// <returns>解析に成功すればtrue，解析に失敗すればfalse</returns>
    public static bool TryParseRoomJoinLog(string log, [NotNullWhen(true)] out RoomJoinLog? room)
    {
        var match = RoomJoinLogPattern.Match(log);
        if (!match.Success)
        {
            room = null;
            return false;
        }

        room = new RoomJoinLog(match.Groups["name"].Value);
        return true;
    }
}

/// <summary>
/// プレイヤーJoinログ
/// </summary>
/// <param name="PlayerName">プレイヤー名</param>
/// <param name="IsLocal">ローカルプレイヤーであればtrue，リモートプレイヤーであればfalse</param>
public record PlayerJoinLog(string PlayerName, bool IsLocal);

/// <summary>
/// プレイヤーLeftログ
/// </summary>
/// <param name="PlayerName">プレイヤー名</param>
public record PlayerLeftLog(string PlayerName);

/// <summary>
/// ルームJoinログ
/// </summary>
/// <param name="WorldName">ワールド名</param>
public record RoomJoinLog(string WorldName);

