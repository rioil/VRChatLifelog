using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Microsoft.EntityFrameworkCore;
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
        internal MainWindowViewModel(LifelogContext lifelogContext)
        {
            _lifelogContext = lifelogContext;
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
                    .ToArray();

                // TODO 期間指定の反映
                var userNames = joinLeaveHistories.Select(h => h.PlayerName).Distinct().OrderBy(name => name);
                MatchedUserNames = new ObservableCollection<string>(userNames);

                // Join/Leave情報から対応するインスタンス情報を取得
                result = joinLeaveHistories
                    .Select(joinleave => _lifelogContext.LocationHistories.Where(l => l.Joined <= joinleave.Joined).OrderByDescending(l => l.Joined).First())
                    .AsQueryable();
            }
            else
            {
                result = _lifelogContext.LocationHistories.AsQueryable();
            }

            // 日付による絞り込み
            if (FilterByDate && (First is not null || Last is not null))
            {
                var end = Last + TimeSpan.FromDays(1);

                result = result
                    .Where(h => First <= h.Joined && (h.Left == null || h.Left < end))
                    .OrderByDescending(h => h.Joined);
            }

            LocationHistories = new ObservableCollection<LocationHistory>(result.ToArray());
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

        /// <summary>
        /// ユーザー名の選択を反映します．
        /// </summary>
        public void SelectUserName(string userName)
        {
            QueriedPerson = userName;
            ApplyFilter();
        }
        private ListenerCommand<string> _selectUserNameCommand;
        public ListenerCommand<string> SelectUserNameCommand => _selectUserNameCommand ??= new ListenerCommand<string>(SelectUserName);
        #endregion

        /// <summary>
        /// Join/Leave履歴を更新します．
        /// </summary>
        /// <param name="location">場所情報</param>
        private void UpdateJoinLeaveHistory(LocationHistory location)
        {
            IEnumerable<JoinLeaveHistory> items;
            if (location.Left is null)
            {
                items = _lifelogContext.JoinLeaveHistories
                    .Where(h => location.Joined <= h.Joined)
                    .OrderBy(h => h.Joined);
            }
            else
            {
                items = _lifelogContext.JoinLeaveHistories
                    .Where(h => location.Joined <= h.Joined && h.Joined <= location.Left)
                    .OrderBy(h => h.Joined);
            }

            JoinLeaveHistories = new ObservableCollection<JoinLeaveHistory>(items);
        }
    }
}
