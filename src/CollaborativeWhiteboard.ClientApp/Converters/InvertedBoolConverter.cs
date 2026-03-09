using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace CollaborativeWhiteboard.ClientApp.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}
