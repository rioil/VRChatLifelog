using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VRChatLifelog.Controls.JoinLeaveTimeline;

public partial class JoinLeaveTimelineItem : UserControl
{
    public JoinLeaveTimelineItem()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
        nameof(LineBrush), typeof(Brush), typeof(JoinLeaveTimelineItem),
        new PropertyMetadata(null, OnLineBrushChanged));

    private static void OnLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not JoinLeaveTimelineItem item) return;
        item.Line.Background = item.LineBrush;
    }

    public Brush LineBrush
    {
        get => (Brush) GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(nameof(Click),
        RoutingStrategy.Bubble, typeof(TimelineItemClickedEventHandler), typeof(JoinLeaveTimelineItem));

    public event TimelineItemClickedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private void OnItemClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: TimelineEvent timelineEvent }) return;
        var args = new TimelineItemClickedEventArgs(ClickEvent, this, timelineEvent);
        RaiseEvent(args);
    }
}

public delegate void TimelineItemClickedEventHandler(object sender, TimelineItemClickedEventArgs args);

public class TimelineItemClickedEventArgs(RoutedEvent routedEvent, object source, TimelineEvent item)
    : RoutedEventArgs(routedEvent, source)
{
    public TimelineEvent Item { get; } = item;
}