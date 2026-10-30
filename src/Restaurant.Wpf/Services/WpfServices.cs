using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Restaurant.Client.Core;
using Restaurant.Contracts;

namespace Restaurant.Wpf.Services;

public sealed class WpfUiDispatcher : IUiDispatcher
{
    public Task InvokeAsync(Action action)
    {
        if (Application.Current?.Dispatcher is { } dispatcher)
        {
            return dispatcher.InvokeAsync(action).Task;
        }
        action();
        return Task.CompletedTask;
    }
}

public sealed class WpfDialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.OK);
    }
}

public sealed class WpfNotificationSound : INotificationSound
{
    public Task PlayAsync(string sound, CancellationToken cancellationToken = default)
    {
        try
        {
            System.Media.SystemSounds.Asterisk.Play();
        }
        catch { }
        return Task.CompletedTask;
    }
}

public sealed class WpfClipboardService : IClipboardService
{
    public Task SetTextAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch { }
        return Task.CompletedTask;
    }
}

public sealed class WpfFilePickerService : IFilePickerService
{
    public Task<FileResult?> PickImageAsync(CancellationToken cancellationToken = default)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn hình ảnh",
            Filter = "Hình ảnh (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|Tất cả tệp (*.*)|*.*",
            Multiselect = false,
        };

        if (dialog.ShowDialog() == true && File.Exists(dialog.FileName))
        {
            var fileInfo = new FileInfo(dialog.FileName);
            var stream = File.OpenRead(dialog.FileName);
            var result = new FileResult(fileInfo.Name, stream, fileInfo.Length);
            return Task.FromResult<FileResult?>(result);
        }

        return Task.FromResult<FileResult?>(null);
    }
}

public sealed class WpfImageCache : IImageCache
{
    private static readonly HttpClient httpClient = new();
    private readonly string cacheDir;

    public WpfImageCache()
    {
        cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RestaurantApp",
            "ImageCache"
        );
        Directory.CreateDirectory(cacheDir);
    }

    public async Task<string?> GetLocalPathAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            var hash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(url)));
            var ext = Path.GetExtension(url);
            if (string.IsNullOrEmpty(ext)) ext = ".png";
            var localPath = Path.Combine(cacheDir, $"{hash}{ext}");

            if (File.Exists(localPath)) return localPath;

            var bytes = await httpClient.GetByteArrayAsync(url, cancellationToken);
            await File.WriteAllBytesAsync(localPath, bytes, cancellationToken);
            return localPath;
        }
        catch
        {
            return null;
        }
    }
}

public sealed class WpfPrintService : IPrintService
{
    public Task PrintReceiptAsync(ReceiptModel model, CancellationToken cancellationToken = default)
    {
        // Khung in K80 - hoàn thiện ở A4
        return Task.CompletedTask;
    }

    public Task PrintKitchenTicketAsync(KitchenTicketModel model, CancellationToken cancellationToken = default)
    {
        // Khung in bếp - hoàn thiện ở A4
        return Task.CompletedTask;
    }
}
