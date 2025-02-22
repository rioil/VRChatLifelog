using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace VRChatLifelog.Data
{
    /// <summary>
    /// ログの項目
    /// </summary>
    /// <param name="Time">日時</param>
    /// <param name="LogLevel">ログレベル</param>
    /// <param name="Content">内容</param>
    public partial record LogItem(DateTime Time, ELogLevel LogLevel, string Content)
    {
        /// <summary>
        /// ログのヘッダーの正規表現パターン
        /// </summary>
        [GeneratedRegex(@"(?<time>\d{4}.\d{2}.\d{2} \d{2}:\d{2}:\d{2}) (?<logLevel>[^-]*)\s*-\s*(?<content>.*)")]
        public static partial Regex HeaderPattern { get; }

        /// <summary>
        /// ログを解析します．
        /// </summary>
        /// <param name="log">ログ文字列</param>
        /// <param name="item">ログ項目</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool TryParse(string? log, [NotNullWhen(true)] out LogItem? item)
        {
            if (log is null)
            {
                item = null;
                return false;
            }

            var match = HeaderPattern.Match(log);
            if (!match.Success)
            {
                item = null;
                return false;
            }

            if (!DateTime.TryParse(match.Groups["time"].ValueSpan, out var time))
            {
                throw new ArgumentException("invalid time", nameof(log));
            }

            if (!Enum.TryParse<ELogLevel>(match.Groups["logLevel"].ValueSpan, out var logLevel))
            {
                logLevel = ELogLevel.Unknown;
            }

            var content = match.Groups["content"].Value;

            item = new(time, logLevel, content);
            return true;
        }
    }

    public enum ELogLevel
    {
        Debug,
        Log,
        Warning,
        Error,
        Unknown,
    }
}
