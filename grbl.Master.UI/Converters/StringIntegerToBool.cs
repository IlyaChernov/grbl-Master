namespace grbl.Master.UI.Converters
{
    using System;
    using System.Windows.Data;

    public class StringIntegerToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null && value is string s && int.TryParse(s, out var result) && result > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool boolRes && boolRes ? "1": "0";
        }
    }
}