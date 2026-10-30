using System.Windows;
using System.Windows.Controls;

namespace Restaurant.Wpf.Controls;

public partial class RemoteImage : UserControl
{
    public static readonly DependencyProperty ImageUrlProperty =
        DependencyProperty.Register(nameof(ImageUrl), typeof(string), typeof(RemoteImage), new PropertyMetadata(null));

    public string? ImageUrl
    {
        get => (string?)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public RemoteImage()
    {
        InitializeComponent();
    }
}
