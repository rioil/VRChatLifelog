using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRChatLogWathcer.ViewModels;
using VRChatLogWathcer.Views;

namespace VRChatLogWathcer.Models
{
    public sealed class NotifyIconService : BackgroundService
    {
        /// <summary>
        /// Dispose済みか
        /// </summary>
        /// <value>Dispose済みであればtrue</value>
        private bool _isDisposed;

        /// <summary>
        /// 通知バーアイコン
        /// </summary>
        private NotifyIcon? _notifyIcon;

        /// <summary>
        /// メイン画面
        /// </summary>
        private MainWindow? _mainWindow;

        /// <summary>
        /// メイン画面VM
        /// </summary>
        private MainWindowViewModel? _mainWindowViewModel;

        /// <summary>
        /// DIコンテナ
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        public NotifyIconService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CreateNotifyIcon();
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) { return; }

            if (disposing)
            {
                _notifyIcon?.Dispose();
            }

            base.Dispose();

            _isDisposed = true;
        }

        /// <summary>
        /// 通知領域アイコンを作成します．
        /// </summary>
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

            _notifyIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ShowMainWindow();
                }
            };
        }

        /// <summary>
        /// アプリケーションを終了します．
        /// </summary>
        private void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// メイン画面を表示します．
        /// </summary>
        private void ShowMainWindow()
        {
            if (_mainWindow is null)
            {
                _mainWindowViewModel = new MainWindowViewModel(_serviceProvider.GetRequiredService<LifelogContext>());
                _mainWindow = new MainWindow
                {
                    DataContext = _mainWindowViewModel
                };
                _mainWindow.Show();
            }
            else if (_mainWindow.WindowState == System.Windows.WindowState.Minimized)
            {
                _mainWindowViewModel?.Initialize();
                _mainWindow.WindowState = System.Windows.WindowState.Normal;
                _mainWindow.ShowInTaskbar = true;
                _mainWindow.Activate();
            }
        }
    }
}
