using Livet;
using Livet.Commands;
using Livet.EventListeners.WeakEvents;
using Livet.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reactive.Bindings;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using VRChatLogWathcer.Data;
using VRChatLogWathcer.Services;
using VRChatLogWathcer.Utils;
using VRChatLogWathcer.Views;

namespace VRChatLogWathcer.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        internal MainWindowViewModel(IServiceProvider serviceProvider)
        {
            _logWatcher = serviceProvider.GetRequiredService<LogWatcherService>();
            CompositeDisposable.Add(new LivetWeakEventListener<WatchingFileCountChangedEventHandler, WatchingFileCountChangedEventArgs>(
                h => new WatchingFileCountChangedEventHandler(h),
                a => _logWatcher.WatchingFileCountChanged += a,
                a => _logWatcher.WatchingFileCountChanged -= a,
                (sender, args) => DispatcherHelper.UIDispatcher.Invoke(() => WatchingFileCount.Value = args.Count)));
            WatchingFileCount.Value = _logWatcher.WatchingFileCount;

            SelectedLocationHistory = new ReactivePropertySlim<LocationHistory?>();
            CompositeDisposable.Add(SelectedLocationHistory.Subscribe(x =>
            {
                if (SelectedLocationHistory.Value is not null)
                {
                    UpdateJoinLeaveHistory(SelectedLocationHistory.Value);
                }
            }));

            SelectedJoinLeaveHistory = new();

            FilterBySelectedUserCommand = new ReactiveCommand(SelectedJoinLeaveHistory.Select(x => x is not null), false)
                .WithSubscribe(() => SelectUserName(SelectedJoinLeaveHistory.Value!.PlayerName));
            FilterBySelectedWorldCommand = new ReactiveCommand(SelectedLocationHistory.Select(x => x is not null), false)
                .WithSubscribe(() => SelectWorldName(SelectedLocationHistory.Value!.WorldName));

            ShowPicturesTakenCommand = new ReactiveCommand(MatchedUserNames.Select(x => x?.Length == 1), false)
                .WithSubscribe(ShowPicturesTaken);

            ShowPicturesTakenInSelectedLocationCommand = new ReactiveCommand(SelectedLocationHistory.Select(x => x is not null), false)
                .WithSubscribe(() => ShowPicturesTakenHere(SelectedLocationHistory.Value!));
        }

        public void Initialize()
        {
            First.Value = DateTime.Today;
            Last.Value = First.Value + TimeSpan.FromDays(1);
            ApplyFilter();
        }

        /// <summary>
        /// 遷移用メッセージキー
        /// </summary>
        private const string TransitionMessageKey = "Transition";

        /// <summary>
        /// ログ監視サービス
        /// </summary>
        private readonly LogWatcherService _logWatcher;

        #region 変更通知プロパティ
        /// <summary>
        /// 表示期間の開始日
        /// </summary>
        public ReactivePropertySlim<DateTime?> First { get; } = new();

        /// <summary>
        /// 表示期間の終了日
        /// </summary>
        public ReactivePropertySlim<DateTime?> Last { get; } = new();

        /// <summary>
        /// 日付による絞り込みを行うか
        /// </summary>
        public ReactivePropertySlim<bool> FilterByDate { get; } = new(true);

        /// <summary>
        /// 検索対象人物
        /// </summary>
        public ReactivePropertySlim<string?> PersonQuery { get; } = new();

        /// <summary>
        /// 人物による絞り込みを行うか
        /// </summary>
        public ReactivePropertySlim<bool> FilterByPerson { get; } = new();

        /// <summary>
        /// あいまい検索によって一致したユーザー名
        /// </summary>
        public ReactivePropertySlim<string[]?> MatchedUserNames { get; } = new();

        /// <summary>
        /// ワールド名のクエリ
        /// </summary>
        public ReactivePropertySlim<string> WorldNameQuery { get; } = new();

        /// <summary>
        /// ワールド名による絞り込みを行うか
        /// </summary>
        public ReactivePropertySlim<bool> FilterByWorldName { get; } = new();

        /// <summary>
        /// あいまい検索によって一致したワールド名
        /// </summary>
        public ReactivePropertySlim<string[]?> MatchedWorldNames { get; } = new();

        /// <summary>
        /// 場所の履歴
        /// </summary>
        public ReactivePropertySlim<LocationHistory[]?> LocationHistories { get; } = new();

        /// <summary>
        /// 選択された場所
        /// </summary>
        public ReactivePropertySlim<LocationHistory?> SelectedLocationHistory { get; }

        /// <summary>
        /// 選択されたプレイヤー
        /// </summary>
        public ReactivePropertySlim<JoinLeaveHistory?> SelectedJoinLeaveHistory { get; }

        /// <summary>
        /// 滞在プレイヤーの履歴
        /// </summary>
        public ReactivePropertySlim<JoinLeaveHistory[]?> JoinLeaveHistories { get; } = new();

        /// <summary>
        /// 監視中のファイル数
        /// </summary>
        public ReactivePropertySlim<int> WatchingFileCount { get; } = new();
        #endregion

        #region コマンド
        /// <summary>
        /// 場所の履歴リストにフィルターを適用して表示項目を絞り込みます．
        /// </summary>
        public async void ApplyFilter()
        {
            IQueryable<LocationHistory> result;
            MatchedUserNames.Value = null;
            MatchedWorldNames.Value = null;
            JoinLeaveHistories.Value = null;

            using var dbContext = new LifelogContext();
            // 対象人物による絞り込み
            if (FilterByPerson.Value && !string.IsNullOrEmpty(PersonQuery.Value))
            {
                // 対象人物のJoin/Leave履歴を取得
                var joinLeaveHistories = await dbContext.JoinLeaveHistories
                    .Where(h => h.PlayerName.Contains(PersonQuery.Value))
                    .Include(h => h.LocationHistory)
                    .ToArrayAsync();

                // TODO 期間指定の反映
                var userNames = joinLeaveHistories.Select(h => h.PlayerName).Distinct().OrderBy(name => name);
                MatchedUserNames.Value = userNames.ToArray();

                // Join/Leave情報から対応するインスタンス情報を取得
                result = joinLeaveHistories
                    .Select(joinleave => joinleave.LocationHistory)
                    .AsQueryable();
            }
            else
            {
                result = dbContext.LocationHistories.AsQueryable();
            }

            // 対象ワールドによる絞り込み
            if (FilterByWorldName.Value && !string.IsNullOrEmpty(WorldNameQuery.Value))
            {
                result = result.Where(l => l.WorldName.Contains(WorldNameQuery.Value));

                // TODO 期間指定の反映
                var worldNames = result.Select(l => l.WorldName).Distinct().OrderBy(name => name);
                MatchedWorldNames.Value = worldNames.ToArray();
            }

            // 日付による絞り込み
            if (FilterByDate.Value && (First.Value is not null || Last.Value is not null))
            {
                if (First.Value is not null)
                {
                    result = result.Where(h => First.Value <= h.Joined);
                }

                var end = Last.Value + TimeSpan.FromDays(1);
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

            LocationHistories.Value = result.ToArray();
            if (SelectedLocationHistory.Value is not null && LocationHistories.Value.Contains(SelectedLocationHistory.Value))
            {
                UpdateJoinLeaveHistory(SelectedLocationHistory.Value);
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
        public ListenerCommand<LocationHistory> ShowPicturesTakenHereCommand =>
            _showPicturesTakenHereCommand ??= new ListenerCommand<LocationHistory>(ShowPicturesTakenHere);
        public ReactiveCommand ShowPicturesTakenInSelectedLocationCommand { get; }

        /// <summary>
        /// 選択された場所で撮影された写真を表示します．
        /// </summary>
        public async void ShowPicturesTaken()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
            // TODO ユーザーがいる間だけに限定できるとより良い
            if (LocationHistories.Value is null) { return; }
            using var dbContext = new LifelogContext();
            var periods = LocationHistories.Value.SelectMany(location =>
            {
                return dbContext.JoinLeaveHistories.Where(x => x.LocationHistoryId == location.Id && x.PlayerName == PersonQuery.Value)
                                                   .Select(x => new SearchMsUtil.Period(x.Joined, x.Left));
            }).ToArray();
            await SearchMsUtil.ShowImages(dir, periods, "Pictures");
        }
        public ReactiveCommand ShowPicturesTakenCommand { get; }

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
        /// <param name="userName">ユーザー名</param>
        public void SelectUserName(string userName)
        {
            PersonQuery.Value = userName;
            FilterByPerson.Value = true;
            ApplyFilter();
        }
        private ListenerCommand<string>? _selectUserNameCandidateCommand;
        public ListenerCommand<string> SelectUserNameCandidateCommand => _selectUserNameCandidateCommand ??= new ListenerCommand<string>(SelectUserName);
        public ReactiveCommand FilterBySelectedUserCommand { get; }

        /// <summary>
        /// ワールド名の選択を反映します．
        /// </summary>
        /// <param name="worldName">ワールド名</param>
        public void SelectWorldName(string worldName)
        {
            WorldNameQuery.Value = worldName;
            FilterByWorldName.Value = true;
            ApplyFilter();
        }
        private ListenerCommand<string>? _selectWorldNameCandidateCommand;
        public ListenerCommand<string> SelectWorldNameCandidateCommand => _selectWorldNameCandidateCommand ??= new ListenerCommand<string>(SelectWorldName);
        public ReactiveCommand FilterBySelectedWorldCommand { get; }
        #endregion

        /// <summary>
        /// Join/Leave履歴を更新します．
        /// </summary>
        /// <param name="location">場所情報</param>
        private void UpdateJoinLeaveHistory(LocationHistory location)
        {
            using var dbContext = new LifelogContext();
            var histories = dbContext.JoinLeaveHistories.Where(h => h.LocationHistory == location).OrderBy(h => h.Joined);
            JoinLeaveHistories.Value = histories.ToArray();
        }
    }
}
