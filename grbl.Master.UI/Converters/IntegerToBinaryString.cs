namespace grbl.Master.UI.Converters
{
    using System;
    using System.Text.RegularExpressions;
    using System.Windows.Data;

    public class IntegerToBinaryString : IValueConverter
    {
        static readonly Regex Binary = new Regex("^[01]{1,32}$", RegexOptions.Compiled);

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

            return System.Convert.ToString(result, 2).PadLeft(3, '0');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string strVal && Binary.IsMatch(strVal))
            {
                return System.Convert.ToInt32(strVal, 2).ToString();
            }

            return 0;
        }
    }
}