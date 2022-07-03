using NLog;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace VRChatLogWathcer.Models
{
    /// <summary>
    /// search-msプロトコルを扱うためのユーティリティクラス
    /// </summary>
    public static class SearchMsUtil
    {
        /// <summary>
        /// 特定期間に作成された画像の検索結果を表示します．
        /// </summary>
        /// <param name="directory">検索対象ディレクトリ</param>
        /// <param name="since">検索範囲の開始日時</param>
        /// <param name="until">検索範囲の終了日時</param>
        /// <param name="title">検索結果のタイトル</param>
        /// <returns></returns>
        public static async Task<bool> ShowImages(string directory, DateTime since, DateTime? until, string title)
        {
            // 検索パラメーターを作成
            var searchParameters = new StringBuilder();
            const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            // TODO 「作成日時」は多言語対応可能？
            const string KeywordCreated = "作成日時";
            var sinceQuery = $"{KeywordCreated}:>={since.ToString(DateTimeFormat)}";
            var untilQuery = until is null ? string.Empty : $" AND {KeywordCreated}:<={until.Value.ToString(DateTimeFormat)}";
            searchParameters.Append($"query={sinceQuery}{untilQuery}");
            searchParameters.Append('&');
            searchParameters.Append($"crumb=location:{directory}");
            searchParameters.Append('&');
            searchParameters.Append("crumb=kind:=pics");
            searchParameters.Append('&');
            searchParameters.Append($"displayname={title}");

            // プロセス実行情報を作成
            // explorer.exeにsearch-msプロトコルのアドレスを入力すると検索結果画面が表示される
            var info = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = $"\"search-ms://{searchParameters}\"",
            };

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
    }
}
