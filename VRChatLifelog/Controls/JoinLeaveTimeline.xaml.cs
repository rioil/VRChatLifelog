using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Livet;
using VRChatLifelog.Data;

namespace VRChatLifelog.Controls;

public partial class JoinLeaveTimeline : UserControl
{
    private readonly ObservableCollection<TimelineItem> _items = [];
    private TimelineEvent? _selectedEvent;

    public JoinLeaveTimeline()
    {
        InitializeComponent();
        TimelineItems.ItemsSource = _items;
    }

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

    private void OnTimelineEventSelected(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: TimelineEvent timelineEvent }) return;

        timelineEvent.IsSelected = !timelineEvent.IsSelected;
        if (_selectedEvent != null && _selectedEvent != timelineEvent)
        {
            _selectedEvent.IsSelected = false;
        }

        _selectedEvent = timelineEvent.IsSelected ? timelineEvent : null;

        var activeFrom = _selectedEvent?.History.Joined ?? DateTime.MinValue;
        var activeTo = _selectedEvent?.History.Left ?? DateTime.MaxValue;
        foreach (var item in _items)
        {
            item.DisplayType = (activeFrom <= item.Time, item.Time <= activeTo) switch
            {
                (true, true) => ETimelineDisplayType.Included,
                (true, false) => ETimelineDisplayType.ExcludedAfter,
                (false, _) => ETimelineDisplayType.ExcludedBefore,
            };
            foreach (var joinEvent in item.JoinEvents)
            {
                joinEvent.TimelineDisplayType = item.DisplayType;
            }
            foreach (var leaveEvent in item.LeaveEvents)
            {
                leaveEvent.TimelineDisplayType = item.DisplayType;
            }
        }
    }
}

public class TimelineEvent(DateTime time, JoinLeaveHistory history, bool isJoin) : NotificationObject
{
    public DateTime Time { get; init; } = time;
    public JoinLeaveHistory History { get; init; } = history;
    public bool IsJoin { get; init; } = isJoin;

    public bool IsSelected
    {
        get => _isSelected;
        set => RaisePropertyChangedIfSet(ref _isSelected, value);
    }

    private bool _isSelected;
    
    public ETimelineDisplayType TimelineDisplayType
    {
        get => _timelineDisplayType;
        set => RaisePropertyChangedIfSet(ref _timelineDisplayType, value);
    }

    private ETimelineDisplayType _timelineDisplayType = ETimelineDisplayType.Included;
}

public class TimelineItem(DateTime time, string formattedTime, TimelineEvent[] joinEvents, TimelineEvent[] leaveEvents)
    : NotificationObject
{
    public DateTime Time { get; init; } = time;
    public string FormattedTime { get; init; } = formattedTime;
    public TimelineEvent[] JoinEvents { get; init; } = joinEvents;
    public TimelineEvent[] LeaveEvents { get; init; } = leaveEvents;

    public ETimelineDisplayType DisplayType
    {
        get => _displayType;
        set => RaisePropertyChangedIfSet(ref _displayType, value);
    }

    private ETimelineDisplayType _displayType = ETimelineDisplayType.Included;
}

public enum ETimelineDisplayType
{
    Included,
    ExcludedBefore,
    ExcludedAfter,
}