using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Restaurant.Client.Core;
using Restaurant.Wpf.Services;
using Restaurant.Wpf.Windows;

namespace Restaurant.Wpf;

public partial class App : Application
{
    private IHost? host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Đảm bảo định dạng số và ngày tháng chuẩn tiếng Việt trên toàn bộ ứng dụng
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("vi-VN"))
        );

        var fake = e.Args.Any(value => string.Equals(value, "--fake", StringComparison.OrdinalIgnoreCase));
        var builder = Host.CreateApplicationBuilder(e.Args);

        // Đăng ký tầng Client.Core
        builder.Services.AddClientCore(fake);

        // Ghi đè các dịch vụ giao diện bằng triển khai WPF thực tế
        builder.Services.AddSingleton<IUiDispatcher, WpfUiDispatcher>();
        builder.Services.AddSingleton<IDialogService, WpfDialogService>();
        builder.Services.AddSingleton<INotificationSound, WpfNotificationSound>();
        builder.Services.AddSingleton<IClipboardService, WpfClipboardService>();
        builder.Services.AddSingleton<IFilePickerService, WpfFilePickerService>();
        builder.Services.AddSingleton<IImageCache, WpfImageCache>();
        builder.Services.AddSingleton<IPrintService, WpfPrintService>();

        // Đăng ký cửa sổ chính ShellWindow
        builder.Services.AddSingleton<ShellWindow>();

        host = builder.Build();
        await host.StartAsync();

        // Khởi động Shell
        var shellWindow = host.Services.GetRequiredService<ShellWindow>();
        MainWindow = shellWindow;
        shellWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (host is not null)
        {
            await host.StopAsync();
            host.Dispose();
        }
        base.OnExit(e);
    }
}
