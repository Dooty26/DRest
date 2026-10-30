using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Restaurant.Wpf.Controls;

public partial class MoneyText : UserControl
{
    public static readonly DependencyProperty AmountProperty =
        DependencyProperty.Register(nameof(Amount), typeof(decimal), typeof(MoneyText), new PropertyMetadata(0m, OnAmountChanged));

    public static readonly DependencyProperty CustomForegroundProperty =
        DependencyProperty.Register(nameof(CustomForeground), typeof(Brush), typeof(MoneyText), new PropertyMetadata(null));

    public decimal Amount
    {
        get => (decimal)GetValue(AmountProperty);
        set => SetValue(AmountProperty, value);
    }

    public Brush? CustomForeground
    {
        get => (Brush?)GetValue(CustomForegroundProperty);
        set => SetValue(CustomForegroundProperty, value);
    }

    public MoneyText()
    {
        InitializeComponent();
        UpdateColor();
    }

    private static void OnAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MoneyText mt)
        {
            mt.UpdateColor();
        }
    }

    private void UpdateColor()
    {
        if (Amount < 0)
        {
            CustomForeground = Application.Current.TryFindResource("DangerBrush") as Brush ?? Brushes.Red;
        }
        else
        {
            CustomForeground = Application.Current.TryFindResource("InkBrush") as Brush ?? Brushes.Black;
        }
    }
}
