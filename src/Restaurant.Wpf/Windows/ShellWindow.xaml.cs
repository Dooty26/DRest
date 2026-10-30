using System.Windows;
using Restaurant.Client.Core;

namespace Restaurant.Wpf.Windows;

public partial class ShellWindow : Window
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) =>
        {
            if (viewModel.CurrentPage is null)
            {
                viewModel.NavigateCommand.Execute("floor");
            }
        };
    }
}
