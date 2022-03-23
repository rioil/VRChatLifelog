using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VRChatLogWathcer.Models;
using VRChatLogWathcer.Views;

namespace VRChatLogWathcer.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        public void Initialize()
        {
            First = DateTime.Today;
            Last = First + TimeSpan.FromDays(1);
            ApplyFilter();
        }

        /// <summary>
        /// 遷移用メッセージキー
        /// </summary>
        private const string TransitionMessageKey = "Transition";

        /// <summary>
        /// 表示期間の開始日
        /// </summary>
        public DateTime? First
        {
            get => _start;
            set => RaisePropertyChangedIfSet(ref _start, value);
        }
        private DateTime? _start;

        /// <summary>
        /// 表示期間の終了日
        /// </summary>
        public DateTime? Last
        {
            get => _end;
            set => RaisePropertyChangedIfSet(ref _end, value);
        }
        private DateTime? _end;

        /// <summary>
        /// 場所の履歴
        /// </summary>
        public ObservableCollection<LocationHistory>? LocationHistories
        {
            get => _locationHistories;
            set => RaisePropertyChangedIfSet(ref _locationHistories, value);
        }
        private ObservableCollection<LocationHistory>? _locationHistories;

        /// <summary>
        /// 選択された場所
        /// </summary>
        public LocationHistory? SelectedLocationHistory
        {
            get => _selectedLocationHistory;
            set
            {
                if (RaisePropertyChangedIfSet(ref _selectedLocationHistory, value) && value is not null)
                {
                    using var context = new LifelogContext();

                    IEnumerable<JoinLeaveHistory> items;
                    if (value.Left is null)
                    {
                        items = context.JoinLeaveHistories
                            .Where(h => value.Joined <= h.Joined)
                            .OrderBy(h => h.Joined);
                    }
                    else
                    {
                        items = context.JoinLeaveHistories
                            .Where(h => value.Joined <= h.Joined && h.Joined <= value.Left)
                            .OrderBy(h => h.Joined);
                    }

                    JoinLeaveHistories = new ObservableCollection<JoinLeaveHistory>(items);
                }
            }
        }
        private LocationHistory? _selectedLocationHistory;

        /// <summary>
        /// 滞在プレイヤーの履歴
        /// </summary>
        public ObservableCollection<JoinLeaveHistory>? JoinLeaveHistories
        {
            get => _joinLeaveHistory;
            set => RaisePropertyChangedIfSet(ref _joinLeaveHistory, value);
        }
        private ObservableCollection<JoinLeaveHistory>? _joinLeaveHistory;

        /// <summary>
        /// 場所の履歴リストにフィルターを適用して表示項目を絞り込みます．
        /// </summary>
        public void ApplyFilter()
        {
            var end = Last + TimeSpan.FromDays(1);

            using var context = new LifelogContext();
            var items = context.LocationHistories
                .Where(h => First <= h.Joined && (h.Left == null || h.Left < end))
                .OrderBy(h => h.Joined);

            LocationHistories = new ObservableCollection<LocationHistory>(items);
        }
        private ViewModelCommand? _applyFilterCommand;
        public ViewModelCommand ApplyFilterCommand => _applyFilterCommand ??= new ViewModelCommand(ApplyFilter);

        /// <summary>
        /// 設定画面を表示します．
        /// </summary>
        public async void OpenSettingWindow()
        {
            var vm = new SettingWindowViewModel();
            await Messenger.RaiseAsync(new TransitionMessage(typeof(SettingWindow), vm, TransitionMode.Modal, TransitionMessageKey));
        }
        private ViewModelCommand? _openSettingWindowCommand;
        public ViewModelCommand OpenSettingWindowCommand => _openSettingWindowCommand ??= new ViewModelCommand(OpenSettingWindow);
    }
}
