using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using VRChatLifelog.Data;

namespace VRChatLifelog.Models;

public static partial class VRChatLogUtil
{
    /// <summary>
    /// プレイヤーJoinログの正規表現パターン
    /// </summary>
    [GeneratedRegex(@"\[Behaviour\] OnPlayerJoined (?<player>.*) \((?<playerId>.*)\)")]
    private static partial Regex PlayerJoinLogPattern { get; }

    /// <summary>
    /// プレイヤーAPI初期化完了ログの正規表現パターン
    /// </summary>
    [GeneratedRegex(@"\[Behaviour\] Initialized PlayerAPI ""(?<player>.*)"" is (?<type>(remote)|(local))")]
    private static partial Regex PlayerInitializedLogPattern { get; }

    /// <summary>
    /// プレイヤーLeftログの正規表現パターン
    /// </summary>
    [GeneratedRegex(@"\[Behaviour\] OnPlayerLeft (?<player>.*) \((?<playerId>.*)\)")]
    private static partial Regex PlayerLeftLogPattern { get; }

    /// <summary>
    /// ワールドJoinログの正規表現パターン
    /// </summary>
    [GeneratedRegex(
        @"\[Behaviour\] Joining (?<worldId>wr?ld_[\da-fA-F]{8}\-[\da-fA-F]{4}\-[\da-fA-F]{4}\-[\da-fA-F]{4}\-[\da-fA-F]{12}):(?<instanceId>\w+)" +
        @"(~(?<type>[\w]+)\((?<master>((usr|grp)_[\da-fA-F]{8}\-[\da-fA-F]{4}\-[\da-fA-F]{4}\-[\da-fA-F]{4}\-[\da-fA-F]{12})|\w{10})\))?" +
        @"(?<canReqInvite>~canRequestInvite)?" +
        @"(~groupAccessType\((?<groupAccessType>[\w]+)\))?" +
        @"(~region\((?<region>[\w]+)\))?" +
        @"(~nonce\((.+)\))?")]
    private static partial Regex WorldJoinLogPattern { get; }

    /// <summary>
    /// ローカルテストワールドJoinログの正規表現パターン
    /// </summary>
    [GeneratedRegex(@"\[Behaviour\] Joining local:(?<worldId>[a-f\d]+)")]
    private static partial Regex LocalTestWorldJoinLogPattern { get; }

    /// <summary>
    /// ルームJoinログの正規表現パターン
    /// </summary>
    [GeneratedRegex(@"\[Behaviour\] Joining or Creating Room: (?<name>.*)")]
    private static partial Regex RoomJoinLogPattern { get; }

    /// <summary>
    /// プレイヤーAPI初期化ログを解析します．
    /// </summary>
    /// <param name="log">ログ文字列</param>
    /// <param name="initializedLog">解析したプレイヤーAPI初期化ログ</param>
    /// <returns>解析に成功すればtrue，解析に失敗すればfalse</returns>
    public static bool TryParsePlayerInitializedLog(string log, [NotNullWhen(true)] out PlayerInitializedLog? initializedLog)
    {
        var match = PlayerInitializedLogPattern.Match(log);
        if (!match.Success)
        {
            initializedLog = null;
            return false;
        }

        var playerName = match.Groups["player"].Value;
        var isLocal = match.Groups["type"].Value == "local";

        initializedLog = new PlayerInitializedLog(playerName, isLocal);
        return true;
    }

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
        var playerId = match.Groups["playerId"].Value;

        joinLog = new PlayerJoinLog(playerName, playerId);
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
        var playerId = match.Groups["playerId"].Value;

        leftLog = new PlayerLeftLog(playerName, playerId);
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
            match = LocalTestWorldJoinLogPattern.Match(log);
            if (!match.Success)
            {
                instance = null;
                return false;
            }

            instance = new Instance(match.Groups["worldId"].Value, string.Empty, EInstanceType.Unknown, ERegion.Unknown, string.Empty);
            return true;
        }

        var worldId = match.Groups["worldId"].Value;
        var instanceId = match.Groups["instanceId"].Value;

        var instanceType = match.Groups["type"].Value;
        var canReqInvite = !string.IsNullOrEmpty(match.Groups["canReqInvite"].Value);
        var groupAccessType = match.Groups["groupAccessType"].Value;
        var eInstanceType = instanceType switch
        {
            "hidden" => EInstanceType.FriendsPlus,
            "friends" => EInstanceType.Friends,
            "private" => canReqInvite ? EInstanceType.InvitePlus : EInstanceType.Invite,
            "group" => groupAccessType switch
            {
                "members" => EInstanceType.Group,
                "plus" => EInstanceType.GroupPlus,
                "public" => EInstanceType.GroupPublic,
                _ => EInstanceType.Unknown
            },
            "" => EInstanceType.Public,
            _ => EInstanceType.Unknown,
        };

        var region = match.Groups["region"].Value.ToLower();
        var eRegion = region switch
        {
            "us" => ERegion.USW,
            "use" => ERegion.USE,
            "eu" => ERegion.EU,
            "jp" => ERegion.JP,
            _ => ERegion.Unknown,
        };

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
public record PlayerInitializedLog(string PlayerName, bool IsLocal);

/// <summary>
/// プレイヤーJoinログ
/// </summary>
/// <param name="PlayerName">プレイヤー名</param>
/// <param name="PlayerId">プレイヤーID</param>
public record PlayerJoinLog(string PlayerName, string PlayerId);

/// <summary>
/// プレイヤーLeftログ
/// </summary>
/// <param name="PlayerName">プレイヤー名</param>
/// <param name="PlayerId">プレイヤーID</param>
public record PlayerLeftLog(string PlayerName, string PlayerId);

/// <summary>
/// ルームJoinログ
/// </summary>
/// <param name="WorldName">ワールド名</param>
public record RoomJoinLog(string WorldName);
