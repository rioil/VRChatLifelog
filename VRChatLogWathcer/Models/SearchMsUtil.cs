using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRChatLogWathcer.Models.SearchMs;

namespace VRChatLogWathcer.Models
{
    /// <summary>
    /// search-msプロトコルを扱うためのユーティリティクラス
    /// </summary>
    public static class SearchMsUtil
    {
        /// <summary>
        /// search-msスキーマのクエリで用いる日時フォーマット
        /// </summary>
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 作成日時を表すキーワード
        /// </summary>
        private const string KeywordCreated = "System.DateCreated";

        /// <summary>
        /// search-msスキーマのクエリの最大長
        /// </summary>
        private const int QueryMaxLength = 255;

        /// <summary>
        /// 特定期間に作成された画像の検索結果を表示します．
        /// </summary>
        /// <param name="directory">検索対象ディレクトリ</param>
        /// <param name="since">検索範囲の開始日時</param>
        /// <param name="until">検索範囲の終了日時</param>
        /// <param name="title">検索結果のタイトル</param>
        /// <returns></returns>
        public static Task<bool> ShowImages(string directory, DateTime since, DateTime? until, string title)
        {
            return ShowImages(directory, new Period[] { new Period(since, until) }, title);
        }

        /// <summary>
        /// 特定期間に作成された画像の検索結果を表示します．
        /// </summary>
        /// <param name="directory">検索対象ディレクトリ</param>
        /// <param name="periods">検索範囲リスト</param>
        /// <param name="title">検索結果のタイトル</param>
        /// <returns></returns>
        public static async Task<bool> ShowImages(string directory, IEnumerable<Period> periods, string title)
        {
            // 検索パラメーターを作成
            var searchParameters = new StringBuilder();
            searchParameters.Append($"query=");
            if (TryAppendPeriodQuery(searchParameters, periods))
            {
                searchParameters.Append('&');
                searchParameters.Append($"crumb=location:{directory}");
                searchParameters.Append('&');
                searchParameters.Append("crumb=kind:=pics");
                searchParameters.Append('&');
                searchParameters.Append($"displayname={title}");
            }

            ProcessStartInfo info;
            // クエリの長さが最大長を超える場合はsearch-msファイルを作成する方式に切り替え
            if (searchParameters.Length > QueryMaxLength)
            {
                var path = PathUtil.CreateTempFileWithExtension("search-ms");
                CreateSearchMsFile(directory, periods, path);

                info = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = path,
                };
            }
            // クエリの長さが最大長以下であればsearch-msスキーマで処理する
            else
            {
                // explorer.exeにsearch-msプロトコルのアドレスを入力すると検索結果画面が表示される
                info = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"search-ms://{searchParameters}\"",
                };
            }

            try
            {
                using var process = Process.Start(info);
                if (process is null) { return false; }

                await process.WaitForExitAsync();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                return false;
            }
        }

        /// <summary>
        /// 指定された条件に基づいてsearch-msファイルを作成します．
        /// </summary>
        /// <param name="directory">検索対象ディレクトリ</param>
        /// <param name="periods">検索対象ファイルの作成日時の期間リスト（OR）</param>
        /// <param name="fileName">作成するsearch-msファイルの名前</param>
        public static void CreateSearchMsFile(string directory, IEnumerable<Period> periods, string fileName)
        {
            // クエリを作成
            var searchms = new PersistedQuery(
                "1.0",
                new ViewInfo(ViewMode.Icons, 96),
                new Query(
                    new Scope(new List<ScopeElement> { new Include(directory, null, false) }),
                    new List<Kind> { new Kind(KindName.Picture) },
                    new List<Condition> {
                        new OrCondition(periods.Select(CreateConditionFromPeriod).ToList())
                    }),
                new SearchMs.Properties());

            // クエリをXMLにシリアライズしてファイルに書き込み
            using var writer = File.Open(fileName, FileMode.Create, FileAccess.Write);
            var serializer = new XmlSerializer(typeof(PersistedQuery));
            serializer.Serialize(writer, searchms);

            // 期間から条件を作成
            Condition CreateConditionFromPeriod(Period period)
            {
                // 開始日時のみ指定されていれば，単一の条件を返す
                if (period.Until is null)
                {
                    return new LeafCondition(CanonicalProperty.DateCreated, "string", Operator.GreaterOrEqual, ConvertDateTime(period.Since), ConditionValueType.DateTime);
                }

                // 開始日時と終了日時が両方指定されていれば，範囲条件を返す
                return new AndCondition(new List<Condition>
                {
                    new LeafCondition(CanonicalProperty.DateCreated, "string", Operator.GreaterOrEqual, ConvertDateTime(period.Since), ConditionValueType.DateTime),
                    new LeafCondition(CanonicalProperty.DateCreated, "string", Operator.LessOrEqual, ConvertDateTime(period.Until.Value), ConditionValueType.DateTime),
                });
            }

            // 日時を条件として利用可能なフォーマットに整形
            string ConvertDateTime(DateTime dateTime)
            {
                return $"N10K{dateTime.Year}K{dateTime.Month}K{dateTime.Day}K{dateTime.Hour}K{dateTime.Minute}K{dateTime.Second}UUZZNNU";
            }
        }

        /// <summary>
        /// 期間指定のクエリを末尾に付加します．
        /// </summary>
        /// <param name="builder">付加対象の<see cref="StringBuilder"/></param>
        /// <param name="periods">期間</param>
        /// <returns>処理に成功すればtrue．クエリの長さが<see cref="QueryMaxLength"/>を超え，処理を中断した場合はfalse．</returns>
        private static bool TryAppendPeriodQuery(StringBuilder builder, IEnumerable<Period> periods)
        {
            bool appendOr = false;
            foreach (var period in periods)
            {
                if (builder.Length >= QueryMaxLength)
                {
                    return false;
                }

                // ORを付加する必要があれば付加する
                if (appendOr)
                {
                    builder.Append(" OR ");
                }
                else
                {
                    appendOr = true;
                }

                // 開始条件を付加
                builder.AppendFormat("({0}:≥{1}", KeywordCreated, period.Since.ToString(DateTimeFormat));

                // 終了条件が存在すれば付加，存在しない場合は括弧を閉じる
                if (period.Until is null)
                {
                    builder.Append(')');
                }
                else
                {
                    builder.AppendFormat(" {0}:≤{1})", KeywordCreated, period.Until.Value.ToString(DateTimeFormat));
                }
            }

            return true;
        }

        /// <summary>
        /// 期間
        /// </summary>
        /// <param name="Since">開始日時（含む）</param>
        /// <param name="Until">終了日時（含む）</param>
        public record Period(DateTime Since, DateTime? Until);
    }
}
