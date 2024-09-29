using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.RestartManager;

namespace VRChatLifelog.Utils
{
    /// <summary>
    /// 再起動マネージャーを利用するためのユーティリティクラスです。
    /// </summary>
    /// <seealso cref="https://learn.microsoft.com/ja-jp/windows/win32/rstmgr/about-restart-manager"/>
    internal static class RestartManagerUtil
    {
        /// <summary>
        /// ファイルをロックしているプロセスを取得します。
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>ファイルをロックしているプロセスのリスト</returns>
        public unsafe static List<Process> GetFileLockingProcesses(string filename)
        {
            var handle = RmStartSession();
            try
            {
                RmRegisterResource(handle, filename);
                return RmGetList(handle);
            }
            finally
            {
                PInvoke.RmEndSession(handle);
            }
        }

        private unsafe static uint RmStartSession()
        {
            var key = Guid.NewGuid().ToString();
            fixed (char* pKey = key)
            {
                var res = PInvoke.RmStartSession(out var handle, new PWSTR(pKey));
                if (res != 0)
                {
                    throw new InvalidOperationException("Failed to start session");
                }

                return handle;
            }
        }

        private unsafe static void RmRegisterResource(uint handle, string filename)
        {
            fixed (char* pFilename = filename)
            {
                var resources = stackalloc PCWSTR[] { new PCWSTR(pFilename) };
                var res = PInvoke.RmRegisterResources(handle, 1, resources, 0, null, 0, null);
                if (res != 0)
                {
                    throw new InvalidOperationException("Failed to register resources");
                }
            }
        }

        private unsafe static List<Process> RmGetList(uint handle)
        {
            uint pnProcInfo = 0;

            var res = PInvoke.RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, null, out _);
            if (res == WIN32_ERROR.ERROR_MORE_DATA)
            {
                var processInfo = stackalloc RM_PROCESS_INFO[(int)pnProcInfoNeeded];
                pnProcInfo = pnProcInfoNeeded;

                res = PInvoke.RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, out _);
                if (res == 0)
                {
                    var processes = new List<Process>((int)pnProcInfo);
                    for (var i = 0; i < pnProcInfo; i++)
                    {
                        var info = processInfo[i];
                        try
                        {
                            processes.Add(Process.GetProcessById((int)info.Process.dwProcessId));
                        }
                        catch (ArgumentException) { }
                    }
                    return processes;
                }
                else
                {
                    throw new InvalidOperationException("Failed to get list");
                }
            }
            else if (res != 0)
            {
                throw new InvalidOperationException("Failed to get size of result");
            }
            else
            {
                return [];
            }
        }
    }
}
