using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VRChatLogWathcer.Views
{
    /* 
     * If some events were receive from ViewModel, then please use PropertyChangedWeakEventListener and CollectionChangedWeakEventListener.
     * If you want to subscribe custome events, then you can use LivetWeakEventListener.
     * When window closing and any timing, Dispose method of LivetCompositeDisposable is useful to release subscribing events.
     *
     * Those events are managed using WeakEventListener, so it is not occurred memory leak, but you should release explicitly.
     */
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 再表示時の復元用ウィンドウ位置
        /// </summary>
        private static WindowLocation? WindowLocation { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            RecoverWindowLocation();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowLocation();
        }

        private void SaveWindowLocation()
        {
            WindowLocation = new WindowLocation(RestoreBounds, WindowState == WindowState.Maximized);
        }

        private void RecoverWindowLocation()
        {
            // Window状態復元
            if (WindowLocation is not null)
            {
                Left = WindowLocation.Rect.Left;
                Top = WindowLocation.Rect.Top;
                Width = WindowLocation.Rect.Width;
                Height = WindowLocation.Rect.Height;

                if (WindowLocation.IsMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
            else
            {
                SaveWindowLocation();
            }
        }
    }

    /// <summary>
    /// ウィンドウ位置
    /// </summary>
    internal class WindowLocation
    {
        /// <summary>
        /// <see cref="Window.RestoreBounds"/>で取得したウィンドウの位置
        /// </summary>
        public Rect Rect { get; set; }

        /// <summary>
        /// 最大化されていたか
        /// </summary>
        public bool IsMaximized { get; set; }

        public WindowLocation(Rect rect, bool isMaximized)
        {
            Rect = rect;
            IsMaximized = isMaximized;
        }
    }
}
