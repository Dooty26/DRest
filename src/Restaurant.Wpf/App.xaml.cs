using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Restaurant.Client.Core;

namespace Restaurant.Wpf;

public partial class App : Application
{
    private IHost? host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var fake = e.Args.Any(value => string.Equals(value, "--fake", StringComparison.OrdinalIgnoreCase));
        var builder = Host.CreateApplicationBuilder(e.Args);
        builder.Services.AddClientCore(fake);
        builder.Services.AddSingleton<MainWindow>();
        host = builder.Build();
        await host.StartAsync();
        MainWindow = host.Services.GetRequiredService<MainWindow>();
        MainWindow.Show();
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
