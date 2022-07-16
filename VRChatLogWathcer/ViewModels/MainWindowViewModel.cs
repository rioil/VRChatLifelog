﻿using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.EventListeners.WeakEvents;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using VRChatLogWathcer.Models;
using VRChatLogWathcer.Views;

namespace VRChatLogWathcer.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        internal MainWindowViewModel(IServiceProvider serviceProvider)
        {
            var lifelogContext = serviceProvider.GetRequiredService<LifelogContext>();
            _lifelogContext = lifelogContext;

            _logWatcher = serviceProvider.GetRequiredService<LogWathcerService>();
            CompositeDisposable.Add(new LivetWeakEventListener<WatchingLogFileChangedEventHandler, WatchingLogFileChangedEventArgs>(
                h => new WatchingLogFileChangedEventHandler(h),
                a => _logWatcher.WatchingFileChanged += a,
                a => _logWatcher.WatchingFileChanged -= a,
                (sender, args) => RaisePropertyChanged(nameof(WatchingFileFullPath))));
        }

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
        /// LifeLog DB
        /// </summary>
        private readonly LifelogContext _lifelogContext;

        /// <summary>
        /// ログ監視サービス
        /// </summary>
        private readonly LogWathcerService _logWatcher;

        #region 変更通知プロパティ
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
        /// 日付による絞り込みを行うか
        /// </summary>
        public bool FilterByDate
        {
            get => _filterByDate;
            set => RaisePropertyChangedIfSet(ref _filterByDate, value);
        }
        private bool _filterByDate = true;

        /// <summary>
        /// 検索対象人物
        /// </summary>
        public string? QueriedPerson
        {
            get => _queriedPerson;
            set => RaisePropertyChangedIfSet(ref _queriedPerson, value);
        }
        private string? _queriedPerson;

        /// <summary>
        /// 人物による絞り込みを行うか
        /// </summary>
        public bool FilterByPerson
        {
            get => _filterByPerson;
            set => RaisePropertyChangedIfSet(ref _filterByPerson, value);
        }
        private bool _filterByPerson;

        /// <summary>
        /// あいまい検索によって一致したユーザー名
        /// </summary>
        public ObservableCollection<string>? MatchedUserNames
        {
            get => _matchedUserNames;
            set => RaisePropertyChangedIfSet(ref _matchedUserNames, value);
        }
        private ObservableCollection<string>? _matchedUserNames;

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
                    UpdateJoinLeaveHistory(value);
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
        /// 開始対象ファイルのフルパス
        /// </summary>
        public string? WatchingFileFullPath => _logWatcher.WatchingFileFullPath;
        #endregion

        #region コマンド
        /// <summary>
        /// 場所の履歴リストにフィルターを適用して表示項目を絞り込みます．
        /// </summary>
        public void ApplyFilter()
        {
            IQueryable<LocationHistory> result;
            MatchedUserNames?.Clear();
            JoinLeaveHistories?.Clear();

            // 対象人物による絞り込み
            if (FilterByPerson && !string.IsNullOrEmpty(QueriedPerson))
            {
                // 対象人物のJoin/Leave履歴を取得
                var joinLeaveHistories = _lifelogContext.JoinLeaveHistories
                    .Where(h => h.PlayerName.Contains(QueriedPerson))
                    .Include(h => h.LocationHistory)
                    .ToArray();

                // TODO 期間指定の反映
                var userNames = joinLeaveHistories.Select(h => h.PlayerName).Distinct().OrderBy(name => name);
                MatchedUserNames = new ObservableCollection<string>(userNames);

                // Join/Leave情報から対応するインスタンス情報を取得
                result = joinLeaveHistories
                    .Select(joinleave => joinleave.LocationHistory)
                    .AsQueryable();
            }
            else
            {
                result = _lifelogContext.LocationHistories.AsQueryable();
            }

            // 日付による絞り込み
            if (FilterByDate && (First is not null || Last is not null))
            {
                if (First is not null)
                {
                    result = result.Where(h => First <= h.Joined);
                }

                var end = Last + TimeSpan.FromDays(1);
                if (end is not null)
                {
                    if (end < DateTime.Now)
                    {
                        result = result.Where(h => h.Left < end);
                    }
                    else
                    {
                        result = result.Where(h => h.Left == null || h.Left < end);
                    }
                }

                result = result.OrderByDescending(h => h.Joined);
            }

            LocationHistories = new ObservableCollection<LocationHistory>(result.ToArray());
            if (SelectedLocationHistory is not null && LocationHistories.Contains(SelectedLocationHistory))
            {
                UpdateJoinLeaveHistory(SelectedLocationHistory);
            }
        }
        private ViewModelCommand? _applyFilterCommand;
        public ViewModelCommand ApplyFilterCommand => _applyFilterCommand ??= new ViewModelCommand(ApplyFilter);

        /// <summary>
        /// 選択された場所で撮影された写真を表示します．
        /// </summary>
        /// <param name="location"></param>
        public async void ShowPicturesTakenHere(LocationHistory location)
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
            var title = location.WorldName;
            await SearchMsUtil.ShowImages(dir, location.Joined, location.Left, title);
        }
        private ListenerCommand<LocationHistory>? _showPicturesTakenHereCommand;
        public ListenerCommand<LocationHistory> ShowPicturesTakenHereCommand => _showPicturesTakenHereCommand ??= new ListenerCommand<LocationHistory>(ShowPicturesTakenHere);

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

        /// <summary>
        /// ユーザー名の選択を反映します．
        /// </summary>
        public void SelectUserName(string userName)
        {
            QueriedPerson = userName;
            FilterByPerson = true;
            ApplyFilter();
        }
        private ListenerCommand<string>? _selectUserNameCommand;
        public ListenerCommand<string> SelectUserNameCommand => _selectUserNameCommand ??= new ListenerCommand<string>(SelectUserName);
        #endregion

        /// <summary>
        /// Join/Leave履歴を更新します．
        /// </summary>
        /// <param name="location">場所情報</param>
        private void UpdateJoinLeaveHistory(LocationHistory location)
        {
            var histories = _lifelogContext.JoinLeaveHistories.Where(h => h.LocationHistory == location).OrderBy(h => h.Joined);
            JoinLeaveHistories = new ObservableCollection<JoinLeaveHistory>(histories);
        }
    }
}
