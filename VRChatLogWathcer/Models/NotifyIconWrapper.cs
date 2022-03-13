using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using VRChatLogWathcer.ViewModels;
using VRChatLogWathcer.Views;

namespace VRChatLogWathcer.Models
{
    public sealed class NotifyIconWrapper : IDisposable
    {
        /// <summary>
        /// Dispose済みか
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 通知バーアイコン
        /// </summary>
        private NotifyIcon _notifyIcon;

        /// <summary>
        /// メイン画面
        /// </summary>
        private MainWindow? _mainWindow;

        public NotifyIconWrapper()
        {
            CreateNotifyIcon();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) { return; }

            if (disposing)
            {
                _notifyIcon?.Dispose();
            }

            _disposed = true;
        }

        [MemberNotNull(nameof(_notifyIcon))]
        private void CreateNotifyIcon()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, (sender, e) => ExitApplication());
            menu.Items.Add("Settings", null, (sender, e) => ShowMainWindow());

            using var icon = System.Windows.Application.GetResourceStream(new Uri("Resources/icon.ico", UriKind.Relative)).Stream;
            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = new System.Drawing.Icon(icon),
                Text = "VRChat Log Watcher",
                ContextMenuStrip = menu,
            };

            _notifyIcon.Click += (sender, e) => ShowMainWindow();
        }

        private void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ShowMainWindow()
        {
            if (_mainWindow is null)
            {
                _mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
                _mainWindow.Show();
            }
            else if (_mainWindow.WindowState == System.Windows.WindowState.Minimized)
            {
                _mainWindow.WindowState = System.Windows.WindowState.Normal;
                _mainWindow.ShowInTaskbar = true;
                _mainWindow.Activate();
            }
        }
    }
}
