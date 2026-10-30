using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Restaurant.Wpf.Converters;

public class VndConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return "0đ";

        decimal amount = 0;
        if (value is decimal d) amount = d;
        else if (value is int i) amount = i;
        else if (value is long l) amount = l;
        else if (value is double db) amount = (decimal)db;
        else if (decimal.TryParse(value.ToString(), out var parsed)) amount = parsed;

        var viCulture = CultureInfo.GetCultureInfo("vi-VN");
        var formatted = string.Format(viCulture, "{0:N0}đ", Math.Abs(amount));

        if (amount < 0)
        {
            if (parameter is string p && p.Equals("label", StringComparison.OrdinalIgnoreCase))
            {
                return $"Hoàn lại khách: {formatted}";
            }
            return $"-{formatted}";
        }

        return formatted;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class TimeSpanToMmSsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            if (ts <= TimeSpan.Zero) return "00:00";
            return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}";
        }
        return "00:00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var b = value is true;
        if (parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase))
        {
            b = !b;
        }
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility v)
        {
            var isVisible = v == Visibility.Visible;
            if (parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase))
            {
                return !isVisible;
            }
            return isVisible;
        }
        return false;
    }
}

public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return Visibility.Collapsed;
        if (value is string s && string.IsNullOrWhiteSpace(s)) return Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class UrlToImageSourceConverter : IValueConverter
{
    private static readonly HttpClient httpClient = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            if (parameter is int decodeWidth && decodeWidth > 0)
            {
                bitmap.DecodePixelWidth = decodeWidth;
            }
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
