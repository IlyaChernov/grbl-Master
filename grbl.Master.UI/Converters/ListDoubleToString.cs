namespace grbl.Master.UI.Converters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;

    public class ListDoubleToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<double> elements)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var element in elements)
                {
                    sb.AppendLine(element.ToString());
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var results = new ObservableCollection<double>();
            if (value is string strVal)
            {
                foreach (var s in strVal.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    results.Add(double.Parse(s));
                }
            }

            return results;
        }
    }
}