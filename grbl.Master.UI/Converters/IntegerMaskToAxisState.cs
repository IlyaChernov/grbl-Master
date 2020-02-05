namespace grbl.Master.UI.Converters
{
    using System;
    using System.Linq;
    using System.Windows.Data;

    public class IntegerMaskToAxisState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = 0;

            if (value != null && value is string s)
            {
                int.TryParse(s, out result);
            }
            else if (value is int intS)
            {
                result = intS;
            }

            if (int.TryParse((string)parameter, out var index) && index >= 0 && index <= 2 && System.Convert.ToString(result, 2).PadLeft(4, '0').Skip(3 - index).Take(1).SingleOrDefault() == '1')
            {
                return "Yes";
            }

            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }
    }
}