using System.Windows;
using System.Windows.Controls;
using Restaurant.Contracts;

namespace Restaurant.Wpf.Controls;

public partial class StatusBadge : UserControl
{
    public static readonly DependencyProperty DisplayStatusProperty =
        DependencyProperty.Register(nameof(DisplayStatus), typeof(TableDisplayStatus), typeof(StatusBadge), new PropertyMetadata(TableDisplayStatus.AVAILABLE));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(StatusBadge), new PropertyMetadata(string.Empty));

    public TableDisplayStatus DisplayStatus
    {
        get => (TableDisplayStatus)GetValue(DisplayStatusProperty);
        set => SetValue(DisplayStatusProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public StatusBadge()
    {
        InitializeComponent();
    }
}
