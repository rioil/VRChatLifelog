using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            Type? comShellType = Type.GetTypeFromProgID("Shell.Application");
            if (comShellType is null)
            {
                hwnd = default;
                return false;
            }

            try
            {
                if (!TryCreateShell(comShellType, out var instance))
                {
                    hwnd = default;
                    return false;
                }
                using var shell = new COMObject(instance);
                using var wins = new COMObject(shell.Object.Windows());
                using var enumerator = new COMObject<IEnumerator>(wins.Object.GetEnumerator());

                while (enumerator.Casted.MoveNext())
                {
                    using var win = new COMObject(enumerator.Casted.Current);
                    if (!win.Object.FullName.Equals(ExplorerPath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var uri = new Uri(win.Object.LocationURL);
                    if (path.Equals(uri.LocalPath, StringComparison.OrdinalIgnoreCase))
                    {
                        hwnd = new IntPtr(win.Object.HWND);
                        return true;
                    }
                }
            }
            catch
            {
                // COM関連で発生した例外は握りつぶす
            }

            hwnd = default;
            return false;

            static bool TryCreateShell(Type type, [NotNullWhen(true)] out object? instance)
            {
                instance = Activator.CreateInstance(type);
                return instance is not null;
            }
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
