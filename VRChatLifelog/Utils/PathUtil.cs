using System.IO;

namespace VRChatLifelog.Utils
{
    public static class PathUtil
    {
        /// <summary>
        /// 指定した拡張子を持つ一時ファイルを作成します．
        /// </summary>
        /// <param name="extension">拡張子（先頭の.の有無は問わない）</param>
        /// <returns>作成した一時ファイルのパス</returns>
        /// <exception cref="IOException">一時ファイルの作成に失敗した場合</exception>
        public static string CreateTempFileWithExtension(string extension)
        {
            for (int i = 0; i < 10; i++)
            {
                var path = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), extension));

                try
                {
                    using var _ = File.Open(path, FileMode.CreateNew);
                    return path;
                }
                catch (IOException)
                {
                }
            }

            throw new IOException("failed to create a temp file");
        }
    }
}
