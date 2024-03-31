using SHDocVw;
using Shell32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VRChatLogWathcer.Utils
{
    internal static class ExplorerUtil
    {
        /// <summary>
        /// エクスプローラーの実行ファイルパス
        /// </summary>
        private const string ExplorerPath = @"C:\Windows\explorer.exe";

        /// <summary>
        /// エクスプローラーで指定したパスを開きます．すでに同じパスを開いているウィンドウが存在する場合は，そのウィンドウをアクティブ化します．
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>操作に成功すればtrue，失敗すればfalse</returns>
        public static bool OpenOrActivate(string path)
        {
            if (TryGetHwndOf(path, out var hwnd))
            {
                if (NativeMethods.IsIconic(hwnd))
                {
                    NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
                }
                return NativeMethods.SetForegroundWindow(hwnd);
            }

            return Open(path);
        }

        /// <summary>
        /// エクスプローラーで指定したパスを開きます．
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns></returns>
        public static bool Open(string path)
        {
            var info = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = path,
            };

            return Process.Start(info) is not null;
        }

        /// <summary>
        /// 指定したパスを開いているエクスプローラーのウィンドウハンドルを取得します．
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns></returns>
        private static bool TryGetHwndOf(string path, out IntPtr hwnd)
        {
            var shell = new Shell();
            ShellWindows wins = shell.Windows();
            foreach (InternetExplorer win in wins)
            {
                if (!win.FullName.Equals(ExplorerPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var uri = new Uri(win.LocationURL);
                if (path.Equals(uri.LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    hwnd = new IntPtr(win.HWND);
                    return true;
                }
            }

            hwnd = default;
            return false;
        }

        private class NativeMethods
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsIconic(IntPtr hWnd);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            public const int SW_RESTORE = 9;
        }
    }
}
