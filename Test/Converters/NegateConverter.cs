using System;
using System.Globalization;
using System.Windows.Data;

namespace Test.Converters;

public class NegateConverter : IValueConverter
{
    public static NegateConverter Instance { get; } = new ();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        => value is not true;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        => this.Convert(value, targetType, parameter, culture);
}