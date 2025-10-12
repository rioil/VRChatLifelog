using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using VRChatLifelog.Data;

namespace VRChatLifelog.Controls;

public partial class LocationHistoryItem : UserControl
{
    public LocationHistoryItem()
    {
        InitializeComponent();
    }
}

[ValueConversion(typeof(LocationHistory), typeof(Brush))]
public class EInstanceTypeToColorConverter : MarkupExtension, IValueConverter
{
    private static readonly Brush PublicBrush;
    private static readonly Brush GroupPublicBrush;
    private static readonly Brush GroupPlusBrush;
    private static readonly Brush GroupBrush;
    private static readonly Brush FriendsPlusBrush;
    private static readonly Brush FriendsBrush;
    private static readonly Brush InvitePlusBrush;
    private static readonly Brush InviteBrush;

    static EInstanceTypeToColorConverter()
    {
        PublicBrush = CreateFrozenBrush(0x00, 0xFF, 0x00); // 明るい緑（最も開放的）
        GroupPublicBrush = CreateFrozenBrush(0x00, 0xFF, 0xFF); // シアン（青緑）
        GroupPlusBrush = CreateFrozenBrush(0x40, 0x80, 0xFF); // 明るい青
        GroupBrush = CreateFrozenBrush(0x00, 0x40, 0xFF); // 濃い青
        FriendsPlusBrush = CreateFrozenBrush(0xFF, 0xA5, 0x00); // オレンジ
        FriendsBrush = CreateFrozenBrush(0x00, 0xBF, 0xFF); // 水色（ディープスカイブルー）
        InvitePlusBrush = CreateFrozenBrush(0xFF, 0x00, 0xFF); // マゼンタ
        InviteBrush = CreateFrozenBrush(0xFF, 0x00, 0x00); // 赤（最も制限的）
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EInstanceType instanceType)
        {
            return GetBrush(instanceType);
        }

        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public static Brush GetBrush(EInstanceType instanceType)
    {
        return instanceType switch
        {
            EInstanceType.Public => PublicBrush,
            EInstanceType.GroupPublic => GroupPublicBrush,
            EInstanceType.GroupPlus => GroupPlusBrush,
            EInstanceType.Group => GroupBrush,
            EInstanceType.FriendsPlus => FriendsPlusBrush,
            EInstanceType.Friends => FriendsBrush,
            EInstanceType.InvitePlus => InvitePlusBrush,
            EInstanceType.Invite => InviteBrush,
            EInstanceType.Unknown => Brushes.Gray,
            _ => Brushes.Transparent,
        };
    }

    private static SolidColorBrush CreateFrozenBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}