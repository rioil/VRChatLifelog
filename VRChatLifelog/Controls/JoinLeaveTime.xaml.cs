using System;
using System.Windows;
using System.Windows.Controls;

namespace VRChatLifelog.Controls
{
    /// <summary>
    /// JoinLeaveTime.xaml の相互作用ロジック
    /// </summary>
    public partial class JoinLeaveTime : UserControl
    {
        public JoinLeaveTime()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 日時フォーマット文字列
        /// </summary>
        private const string DateTimeFormat = "yyyy/MM/dd HH:mm:ss";

        /// <summary>
        /// 時刻のみフォーマット文字列
        /// </summary>
        private const string TimeFormat = "HH:mm:ss";

        /// <summary>
        /// Join時間
        /// </summary>
        public DateTime Join
        {
            get { return (DateTime)GetValue(JoinProperty); }
            set { SetValue(JoinProperty, value); }
        }
        public static readonly DependencyProperty JoinProperty =
            DependencyProperty.Register("Join", typeof(DateTime), typeof(JoinLeaveTime), new PropertyMetadata(default(DateTime), OnJoinChanged));

        /// <summary>
        /// Leave時間
        /// </summary>
        public DateTime? Leave
        {
            get { return (DateTime?)GetValue(LeaveProperty); }
            set { SetValue(LeaveProperty, value); }
        }
        public static readonly DependencyProperty LeaveProperty =
            DependencyProperty.Register("Leave", typeof(DateTime?), typeof(JoinLeaveTime), new PropertyMetadata(default(DateTime?), OnLeaveChanged));

        /// <summary>
        /// <see cref="Join"/>が変化したときの処理を行います．
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnJoinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not JoinLeaveTime instance) { return; }
            instance.JoinTimeText.Text = instance.Join.ToString(DateTimeFormat);
        }

        /// <summary>
        /// <see cref="Leave"/>が変化したときの処理を行います．
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnLeaveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not JoinLeaveTime instance) { return; }
            if (instance.Leave is null)
            {
                instance.LeaveTimeText.Text = null;
            }
            else
            {
                var format = IsSameDay(instance.Join, instance.Leave.Value) ? TimeFormat : DateTimeFormat;
                instance.LeaveTimeText.Text = instance.Leave.Value.ToString(format);
            }
        }

        /// <summary>
        /// 同じ日付の日時であるかを判定します．
        /// </summary>
        /// <param name="date1">1つ目の日付</param>
        /// <param name="date2">2つ目の日付</param>
        /// <returns>日付が一致する場合true，それ以外はfalse</returns>
        private static bool IsSameDay(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day;
        }
    }
}
