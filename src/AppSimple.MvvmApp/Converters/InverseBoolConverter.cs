using System.Globalization;
using Avalonia.Data.Converters;

namespace AppSimple.MvvmApp.Converters;

/// <summary>
/// Returns <see langword="true"/> when the bound value is <see langword="false"/>, and vice versa.
/// Useful for <c>IsVisible="{Binding IsLoggedIn, Converter={x:Static conv:InverseBoolConverter.Instance}}"</c>.
/// </summary>
public sealed class InverseBoolConverter : IValueConverter
{
    /// <summary>Gets the singleton instance.</summary>
    public static readonly InverseBoolConverter Instance = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}
