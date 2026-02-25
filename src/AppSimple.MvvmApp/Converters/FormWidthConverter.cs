using System.Globalization;
using System.Windows.Data;
using AppSimple.Core.Enums;

namespace AppSimple.MvvmApp.Converters;

/// <summary>
/// Returns <c>320</c> (double) when the value is <c>true</c> (form visible),
/// or <c>0</c> when <c>false</c>. Used as MaxWidth on the right-panel column
/// to collapse it when the form is hidden without breaking the layout.
/// </summary>
[ValueConversion(typeof(bool), typeof(double))]
public sealed class FormWidthConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? 320.0 : 0.0;

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
