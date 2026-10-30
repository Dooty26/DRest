using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Restaurant.Contracts;

namespace Restaurant.Wpf.Converters;

public class DisplayStatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value switch
        {
            TableDisplayStatus s => s,
            string str when Enum.TryParse<TableDisplayStatus>(str, true, out var parsed) => parsed,
            _ => TableDisplayStatus.AVAILABLE
        };

        var key = status switch
        {
            TableDisplayStatus.AVAILABLE => "StatusAvailableBrush",
            TableDisplayStatus.RESERVED => "StatusReservedBrush",
            TableDisplayStatus.OCCUPIED => "StatusOccupiedBrush",
            TableDisplayStatus.CLEANING => "StatusCleaningBrush",
            TableDisplayStatus.HELD => "StatusHeldBrush",
            _ => "StatusAvailableBrush"
        };

        return Application.Current.TryFindResource(key) as Brush ?? Brushes.Green;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class DisplayStatusToBackgroundBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value switch
        {
            TableDisplayStatus s => s,
            string str when Enum.TryParse<TableDisplayStatus>(str, true, out var parsed) => parsed,
            _ => TableDisplayStatus.AVAILABLE
        };

        var key = status switch
        {
            TableDisplayStatus.AVAILABLE => "StatusAvailableBgBrush",
            TableDisplayStatus.RESERVED => "StatusReservedBgBrush",
            TableDisplayStatus.OCCUPIED => "StatusOccupiedBgBrush",
            TableDisplayStatus.CLEANING => "StatusCleaningBgBrush",
            TableDisplayStatus.HELD => "StatusHeldBgBrush",
            _ => "StatusAvailableBgBrush"
        };

        return Application.Current.TryFindResource(key) as Brush ?? Brushes.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
