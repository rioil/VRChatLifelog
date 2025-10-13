using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Livet;
using VRChatLifelog.Data;

namespace VRChatLifelog.Controls.JoinLeaveTimeline;

public partial class JoinLeaveTimeline : UserControl
{
    private readonly ObservableCollection<TimelineItem> _items = [];
    private JoinLeaveHistory? _selectedHistory;

    public JoinLeaveTimeline()
    {
        InitializeComponent();
        TimelineItems.ItemsSource = _items;
    }

    #region dependency property

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable<JoinLeaveHistory>), typeof(JoinLeaveTimeline),
        new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable<JoinLeaveHistory>? ItemsSource
    {
        get => (IEnumerable<JoinLeaveHistory>?) GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not JoinLeaveTimeline control) return;
        control.ResetTimelineItems();
    }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(JoinLeaveHistory), typeof(JoinLeaveTimeline),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedItemChanged));

    public JoinLeaveHistory? SelectedItem
    {
        get => (JoinLeaveHistory?) GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not JoinLeaveTimeline control) return;
        control.SelectHistory(control.SelectedItem);
    }

    #endregion

    private void ResetTimelineItems()
    {
        _items.Clear();
        if (ItemsSource == null) return;

        var groupedEvents = ItemsSource
            .SelectMany(ToEvents)
            .GroupBy(e => e.Time)
            .OrderBy(g => g.Key);

        var lastEventTime = DateTime.MinValue;
        foreach (var group in groupedEvents)
        {
            var joined = group
                .Where(e => e.IsJoin)
                .OrderBy(e => e.History.PlayerName)
                .ToArray();
            var left = group
                .Where(e => !e.IsJoin)
                .OrderBy(e => e.History.PlayerName)
                .ToArray();
            var timeFormat = (group.Key.Year == lastEventTime.Year,
                    group.Key.Month == lastEventTime.Month && group.Key.Day == lastEventTime.Day) switch
                {
                    (false, _) => "yyyy/MM/dd HH:mm:ss",
                    (true, false) => "MM/dd HH:mm:ss",
                    (true, true) => "HH:mm:ss",
                };
            var formattedTime = group.Key.ToString(timeFormat);
            _items.Add(new TimelineItem(group.Key, formattedTime, joined, left));
            lastEventTime = group.Key;
        }

        return;

        IEnumerable<TimelineEvent> ToEvents(JoinLeaveHistory history)
        {
            yield return new TimelineEvent(history.Joined, history, true);
            if (history.Left.HasValue)
            {
                yield return new TimelineEvent(history.Left.Value, history, false);
            }
        }
    }

    private void OnTimelineEventClicked(object sender, TimelineItemClickedEventArgs e)
    {
        SelectHistory(e.Item.History == _selectedHistory ? null : e.Item.History);
    }

    private void SelectHistory(JoinLeaveHistory? history)
    {
        if (_selectedHistory == history)
        {
            return;
        }

        _selectedHistory = history;

        var activeFrom = _selectedHistory?.Joined ?? DateTime.MinValue;
        var activeTo = _selectedHistory?.Left ?? DateTime.MaxValue;
        foreach (var item in _items)
        {
            item.ParticipationPhase = (activeFrom <= item.Time, item.Time <= activeTo) switch
            {
                (true, true) => EParticipationPhase.DuringStay,
                (true, false) => EParticipationPhase.AfterLeave,
                (false, _) => EParticipationPhase.BeforeJoin,
            };

            foreach (var joinEvent in item.JoinEvents)
            {
                joinEvent.IsSelected = joinEvent.History == _selectedHistory;
                joinEvent.ParticipationPhase = item.ParticipationPhase;
                joinEvent.IsOverlappedWithSelection = IsOverlapped(joinEvent);
            }

            foreach (var leaveEvent in item.LeaveEvents)
            {
                leaveEvent.IsSelected = leaveEvent.History == _selectedHistory;
                leaveEvent.ParticipationPhase = item.ParticipationPhase;
                leaveEvent.IsOverlappedWithSelection = IsOverlapped(leaveEvent);
            }
        }

        SelectedItem = _selectedHistory;
        return;

        bool? IsOverlapped(TimelineEvent @event)
        {
            if (_selectedHistory == null) return null;
            var target = @event.History;
            var selected = _selectedHistory;

            var leftBeforeJoin = target.Left < selected.Joined;
            var joinedAfterLeft = selected.Left != null && target.Joined > selected.Left;
            return !(leftBeforeJoin || joinedAfterLeft);
        }
    }
}

public class TimelineEvent(DateTime time, JoinLeaveHistory history, bool isJoin) : NotificationObject
{
    public DateTime Time { get; } = time;
    public JoinLeaveHistory History { get; } = history;
    public bool IsJoin { get; } = isJoin;

    public bool IsSelected
    {
        get => _isSelected;
        set => RaisePropertyChangedIfSet(ref _isSelected, value);
    }

    private bool _isSelected;

    public EParticipationPhase ParticipationPhase
    {
        get => _participationPhase;
        set => RaisePropertyChangedIfSet(ref _participationPhase, value);
    }

    private EParticipationPhase _participationPhase = EParticipationPhase.DuringStay;

    public bool? IsOverlappedWithSelection
    {
        get => _isOverlappedWithSelection;
        set => RaisePropertyChangedIfSet(ref _isOverlappedWithSelection, value);
    }

    private bool? _isOverlappedWithSelection;
}

public class TimelineItem(DateTime time, string formattedTime, TimelineEvent[] joinEvents, TimelineEvent[] leaveEvents)
    : NotificationObject
{
    public DateTime Time { get; } = time;
    public string FormattedTime { get; } = formattedTime;
    public TimelineEvent[] JoinEvents { get; } = joinEvents;
    public TimelineEvent[] LeaveEvents { get; } = leaveEvents;

    public EParticipationPhase ParticipationPhase
    {
        get => _participationPhase;
        set => RaisePropertyChangedIfSet(ref _participationPhase, value);
    }

    private EParticipationPhase _participationPhase = EParticipationPhase.DuringStay;
}

public enum EParticipationPhase
{
    DuringStay,
    BeforeJoin,
    AfterLeave,
}