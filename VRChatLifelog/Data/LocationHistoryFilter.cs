using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VRChatLifelog.Data
{
    /// <summary>
    /// 場所の履歴をフィルタリングするための条件
    /// </summary>
    internal class LocationHistoryFilter
    {
        /// <summary>
        /// 日付による絞り込みを行うかどうか
        /// </summary>
        public bool FilterByDate { get; set; }

        /// <summary>
        /// 人物による絞り込みを行うかどうか
        /// </summary>
        public bool FilterByPerson { get; set; }

        /// <summary>
        /// ワールド名による絞り込みを行うかどうか
        /// </summary>
        public bool FilterByWorldName { get; set; }

        /// <summary>
        /// 日付の範囲の開始日時
        /// </summary>
        public DateTime? DateFirst { get; set; }

        /// <summary>
        /// 日付の範囲の終了日時
        /// </summary>
        public DateTime? DateLast { get; set; }

        /// <summary>
        /// 人物による絞り込みのクエリ
        /// </summary>
        public string? PersonQuery { get; set; }

        /// <summary>
        /// ワールド名に夜絞り込みのクエリ
        /// </summary>
        public string? WorldNameQuery { get; set; }

        /// <summary>
        /// 場所の履歴リストからフィルター条件に一致する項目を取得します．
        /// </summary>
        /// <param name="context">履歴データ</param>
        /// <returns>条件に一致したデータ</returns>
        public async ValueTask<FilteredData> ApplyAsync(LifelogContext context)
        {
            IQueryable<LocationHistory> result;
            string[] userNames = [];
            string[] worldNames = [];

            // 対象人物による絞り込み
            if (FilterByPerson && !string.IsNullOrEmpty(PersonQuery))
            {
                // 対象人物のJoin/Leave履歴を取得
                var joinLeaveHistories = context.JoinLeaveHistories
                    .Where(h => h.PlayerName.Contains(PersonQuery));
                /* 
                 * MEMO:
                 * 文字列一致のクエリが複数回実行されることになるが，配列化してキャッシュしてはいけない
                 * 配列はIAsyncEnumerableを実装していないため，LocationHistoriesを作成する最後のToArrayAsyncで例外が発生してしまう
                 */

                // TODO 期間指定の反映
                userNames = await joinLeaveHistories.Select(h => h.PlayerName)
                                                    .Distinct()
                                                    .OrderBy(name => name)
                                                    .ToArrayAsync();

                // Join/Leave情報から対応するインスタンス情報を取得
                result = joinLeaveHistories.Include(h => h.LocationHistory)
                                           .Select(joinleave => joinleave.LocationHistory)
                                           .AsQueryable();
            }
            else
            {
                result = context.LocationHistories.AsQueryable();
            }

            // 対象ワールドによる絞り込み
            if (FilterByWorldName && !string.IsNullOrEmpty(WorldNameQuery))
            {
                result = result.Where(l => l.WorldName.Contains(WorldNameQuery));

                // TODO 期間指定の反映
                worldNames = await result.Select(l => l.WorldName)
                                         .Distinct()
                                         .OrderBy(name => name)
                                         .ToArrayAsync();
            }

            // 日付による絞り込み
            if (FilterByDate && (DateFirst is not null || DateLast is not null))
            {
                if (DateFirst is not null)
                {
                    result = result.Where(h => DateFirst <= h.Joined);
                }

                var end = DateLast + TimeSpan.FromDays(1);
                if (end is not null)
                {
                    if (end < DateTime.Now)
                    {
                        result = result.Where(h => h.Left < end);
                    }
                    else
                    {
                        result = result.Where(h => h.Left == null || h.Left < end);
                    }
                }
            }

            var locationHistories = await result.OrderByDescending(h => h.Joined).ToArrayAsync();

            return new FilteredData(userNames, worldNames, locationHistories);
        }
    }
}
