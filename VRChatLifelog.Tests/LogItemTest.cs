using VRChatLifelog.Data;

namespace VRChatLifelog.Tests
{
    public class LogItemTest
    {
        public static TheoryData<string, ELogLevel, DateTime, string> LogItemTestData { get; } = new() {
            {
                "2025.02.20 07:30:40 Debug      -  [Behaviour] OnPlayerJoined hoge (usr_7c61377d-df7c-4cbe-a486-c8caad0d22de)",
                ELogLevel.Debug,
                new DateTime(2025, 2, 20, 7, 30, 40),
                "[Behaviour] OnPlayerJoined hoge (usr_7c61377d-df7c-4cbe-a486-c8caad0d22de)"
            }
        };

        [Theory]
        [MemberData(nameof(LogItemTestData))]
        public void TestParsePlayerJoinLog(string log, ELogLevel logLevel, DateTime time, string content)
        {
            var isOk = LogItem.TryParse(log, out var header);

            Assert.True(isOk);
            Assert.NotNull(header);
            Assert.Equal(logLevel, header.LogLevel);
            Assert.Equal(time, header.Time);
            Assert.Equal(content, header.Content);
        }
    }
}
