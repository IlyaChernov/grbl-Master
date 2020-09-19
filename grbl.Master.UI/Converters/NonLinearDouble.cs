namespace grbl.Master.UI.Converters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;

    public class NonLinearDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter is Label powLbl && powLbl.Content is double pow)
            {
                pow = pow < 1 ? 1 : pow;
                if (value != null)
                {
                    if (value is double val)
                    {
                        return Math.Pow(val, 1d / pow);
                    }

                    if (value is ObservableCollection<double> vals && vals.Any())
                    {
                        return new DoubleCollection(vals.Select(val => Math.Pow(val, 1d / pow)));
                    }
                }
            }

            return 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is double val && parameter != null && parameter is Label powLbl && powLbl.Content is double pow)
            {
                return Math.Round(Math.Pow(val, pow), 3);
            }
            return 0d;
        }
    }
}