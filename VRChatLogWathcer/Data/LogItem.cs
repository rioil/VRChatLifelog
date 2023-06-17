using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace VRChatLogWathcer.Data
{
    internal class LogItem
    {
        public LogItem(DateTime time, ELogLevel logLevel, string content)
        {
            Time = time;
            LogLevel = logLevel;
            Content = content;
        }

        public DateTime Time { get; }
        public ELogLevel LogLevel { get; }
        public string Content { get; }


        public static bool TryParse(IEnumerable<string> logLines, [NotNullWhen(true)] out LogItem? header)
        {
            //lang=regex
            const string HeaderPattern = @"(?<time>\d{4}.\d{2}.\d{2} \d{2}:\d{2}:\d{2}) (?<logLevel>[^-]*)\s*-\s*(?<content>.*)";

            if (!logLines.Any())
            {
                header = null;
                return false;
            }

            var match = Regex.Match(logLines.First(), HeaderPattern);
            if (match.Success)
            {
                if (!DateTime.TryParse(match.Groups["time"].Value, out var time))
                {
                    throw new ArgumentException("invalid time", nameof(logLines));
                }

                if (!Enum.TryParse<ELogLevel>(match.Groups["logLevel"].Value, out var logLevel))
                {
                    logLevel = ELogLevel.Unknown;
                }

                var content = match.Groups["content"].Value;

                header = new(time, logLevel, content);
                return true;
            }
            else
            {
                header = null;
                return false;
            }
        }
    }

    internal enum ELogLevel
    {
        Log,
        Warning,
        Error,
        Unknown,
    }
}
