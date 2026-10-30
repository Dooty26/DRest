using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Client.Core;
using Restaurant.Wpf.Views;

namespace Restaurant.Wpf.Services;

public class ShellPageTemplateSelector : DataTemplateSelector
{
    public IServiceProvider? Services { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (container is not FrameworkElement fe) return base.SelectTemplate(item, container);

        var key = item switch
        {
            string s => s.ToLowerInvariant(),
            FloorPlanViewModel => "floor",
            PosViewModel => "pos",
            KitchenViewModel => "kitchen",
            ReportsViewModel => "reports",
            ReservationsViewModel => "reservations",
            LoginViewModel => "login",
            _ => "floor"
        };

        var templateKey = $"{key}Template";
        if (fe.TryFindResource(templateKey) is DataTemplate dt)
        {
            return dt;
        }

        return base.SelectTemplate(item, container);
    }
}
