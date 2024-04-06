using System;
using System.Runtime.InteropServices;

namespace VRChatLifelog.Utils
{
    /// <summary>
    /// COMオブジェクトの開放処理を確実に行うためのラッパーオブジェクト
    /// </summary>
    internal class COMObject : IDisposable
    {
        private dynamic? _object;
        private bool _isDisposed;

        public dynamic Object => (_isDisposed || _object is null) ? throw new ObjectDisposedException(nameof(COMObject)) : _object;

        public COMObject(dynamic comObject)
        {
            _object = comObject;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                //if (disposing)
                //{
                //    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                //}

                Marshal.ReleaseComObject(_object);
                _object = null;
                _isDisposed = true;
            }
        }

        ~COMObject()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// COMオブジェクトの開放処理を確実に行うためのラッパーオブジェクト
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class COMObject<T> : IDisposable
    {
        private dynamic? _object;
        private bool _isDisposed;

        public dynamic Object => (_isDisposed || _object is null) ? throw new ObjectDisposedException(nameof(COMObject)) : _object;
        
        public T Casted => (T)Object;

        public COMObject(dynamic comObject)
        {
            _object = comObject;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                //if (disposing)
                //{
                //    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                //}

                Marshal.ReleaseComObject(_object);
                _object = null;
                _isDisposed = true;
            }
        }

        ~COMObject()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static implicit operator T(COMObject<T> obj) => obj.Casted;
    }
}
