namespace VRChatLifelog.Data
{
    /// <summary>
    /// フィルター条件に一致したデータ
    /// </summary>
    /// <param name="MatchedUserNames"> あいまい検索によって一致したユーザー名 </param>
    /// <param name="MatchedWorldNames"> あいまい検索によって一致したワールド名 </param>
    /// <param name="LocationHistories"> 場所の履歴 </param>
    internal record FilteredData(string[] MatchedUserNames, string[] MatchedWorldNames, LocationHistory[] LocationHistories);
}
