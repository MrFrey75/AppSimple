using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AppSimple.MvvmApp.Converters;

/// <summary>
/// Converts a <see cref="bool"/> to an inverted <see cref="Visibility"/>.
/// <c>false</c> → <see cref="Visibility.Visible"/>; <c>true</c> → <see cref="Visibility.Collapsed"/>.
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is not Visibility.Visible;
}
