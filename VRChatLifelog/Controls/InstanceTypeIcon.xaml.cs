using MahApps.Metro.IconPacks;
using MahApps.Metro.IconPacks.Converter;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using VRChatLifelog.Data;

namespace VRChatLifelog.Controls
{
    /// <summary>
    /// InstanceTypeIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class InstanceTypeIcon : UserControl
    {
        public InstanceTypeIcon()
        {
            InitializeComponent();
        }

        private static PackIconCooliconsKindToImageConverter _iconConverter = new();

        /// <summary>
        /// インスタンスタイプ
        /// </summary>
        public EInstanceType InstanceType
        {
            get => (EInstanceType)GetValue(InstanceTypeProperty);
            set => SetValue(InstanceTypeProperty, value);
        }
        public static readonly DependencyProperty InstanceTypeProperty =
            DependencyProperty.Register("InstanceType", typeof(EInstanceType), typeof(InstanceTypeIcon),
                new PropertyMetadata(default(EInstanceType), OnInstanceTypeChanged));

        private static void OnInstanceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not InstanceTypeIcon instance) { return; }

            var mainIconKind = instance.InstanceType switch
            {
                EInstanceType.Public => PackIconCooliconsKind.GroupAlt,
                EInstanceType.Friends or EInstanceType.FriendsPlus => PackIconCooliconsKind.User,
                EInstanceType.Invite or EInstanceType.InvitePlus => PackIconCooliconsKind.Mail,
                EInstanceType.Group or EInstanceType.GroupPlus or EInstanceType.GroupPublic => PackIconCooliconsKind.Group,
                EInstanceType.Unknown => PackIconCooliconsKind.HelpQuestionmark,
                _ => throw new NotImplementedException(),
            };
            var mainIcon = GetIconImage(mainIconKind);
            instance.MainIcon.Source = mainIcon;

            PackIconCooliconsKind? subIconKind = instance.InstanceType switch
            {
                EInstanceType.Public => null,
                EInstanceType.Friends => null,
                EInstanceType.FriendsPlus => PackIconCooliconsKind.Plus,
                EInstanceType.Invite => null,
                EInstanceType.InvitePlus => PackIconCooliconsKind.Plus,
                EInstanceType.Group => null,
                EInstanceType.GroupPlus => PackIconCooliconsKind.Plus,
                EInstanceType.GroupPublic => PackIconCooliconsKind.GroupAlt,
                EInstanceType.Unknown => null,
                _ => throw new NotImplementedException(),
            };

            var subIcon = GetIconImage(subIconKind);
            instance.SubIcon.Source = subIcon;
        }

        private static ImageSource? GetIconImage(PackIconCooliconsKind? iconKind)
        {
            if (iconKind is null) { return null; }
            return ((IValueConverter)_iconConverter).Convert(iconKind, typeof(ImageSource), null, CultureInfo.CurrentCulture) as ImageSource;
        }
    }
}
