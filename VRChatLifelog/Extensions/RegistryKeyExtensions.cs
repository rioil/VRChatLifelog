using Microsoft.Win32;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;

namespace VRChatLifelog.Extensions
{
    /// <summary>
    /// RegistryKeyの拡張メソッドクラス
    /// </summary>
    public static class RegistryKeyExtensions
    {
        /// <summary>
        /// 書き込みアクセスが必要であるかを指定して，サブキーをオープンします．
        /// </summary>
        /// <param name="registryKey">キー</param>
        /// <param name="name">名前</param>
        /// <param name="writable">書き込みアクセスが必要であればtrue</param>
        /// <param name="throwOnSecurityError">権限不足時に例外を送出する場合true</param>
        /// <returns>オープンしたサブキー</returns>
        public static RegistryKey? OpenSubKey(this RegistryKey registryKey, string name, bool writable, bool throwOnSecurityError)
        {
            try
            {
                return registryKey.OpenSubKey(name, writable);
            }
            catch (SecurityException)
            {
                if (throwOnSecurityError) { throw; }
                return null;
            }
        }

        /// <summary>
        /// キーの指定した名前の値を取得します．
        /// </summary>
        /// <param name="registryKey">キー</param>
        /// <param name="name">名前</param>
        /// <param name="value">値</param>
        /// <returns>成功すればtrue</returns>
        public static bool TryGetValue(this RegistryKey registryKey, string name, [NotNullWhen(true)] out string? value)
        {
            try
            {
                value = registryKey.GetValue(name) as string;
                return value is not null;
            }
            catch (Exception ex) when (ex is SecurityException || ex is IOException || ex is UnauthorizedAccessException)
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// キーの指定した名前の値を設定します．
        /// </summary>
        /// <param name="registryKey">キー</param>
        /// <param name="name">名前</param>
        /// <param name="value">値</param>
        /// <param name="registryValueKind">値の種類</param>
        /// <returns>成功すればtrue</returns>
        public static bool TrySetValue(this RegistryKey registryKey, string name, object value, RegistryValueKind registryValueKind)
        {
            try
            {
                registryKey.SetValue(name, value, registryValueKind);
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                return false;
            }
        }

        /// <summary>
        /// キーの指定した名前の値を削除します．
        /// </summary>
        /// <param name="registryKey">キー</param>
        /// <param name="name">名前</param>
        /// <returns>成功すればtrue．指定した名前の値が存在しない場合もtrue．</returns>
        public static bool TryDeleteValue(this RegistryKey registryKey, string name)
        {
            try
            {
                registryKey.DeleteValue(name, false);
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is SecurityException || ex is UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
