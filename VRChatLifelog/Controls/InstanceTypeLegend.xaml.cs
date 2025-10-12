using System.Windows;
using System.Windows.Controls;
using VRChatLifelog.Data;

namespace VRChatLifelog.Controls;

public partial class InstanceTypeLegend : UserControl
{
    public InstanceTypeLegend()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// インスタンスタイプ
    /// </summary>
    public EInstanceType InstanceType
    {
        get => (EInstanceType)GetValue(InstanceTypeProperty);
        set => SetValue(InstanceTypeProperty, value);
    }
    public static readonly DependencyProperty InstanceTypeProperty =
        DependencyProperty.Register(nameof(InstanceType), typeof(EInstanceType), typeof(InstanceTypeLegend),
            new PropertyMetadata(default(EInstanceType), OnInstanceTypeChanged));

    private static void OnInstanceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InstanceTypeLegend instanceTypeLegend) return;
        instanceTypeLegend.Color.Background = EInstanceTypeToColorConverter.GetBrush(instanceTypeLegend.InstanceType);
        instanceTypeLegend.Name.Text = instanceTypeLegend.InstanceType.ToString();
    }
}