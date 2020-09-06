namespace grbl.Master.UI.Converters
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    using grbl.Master.Service.Enum;

    public class EnumToDescription : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null ? DependencyProperty.UnsetValue : GetDescription((Enum)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            return Enum.GetValues(targetType).Cast<Enum>().FirstOrDefault(one => value.ToString() == GetDescription(one));
        }

        public static string GetDescription(Enum en)
        {

            var attr = en.GetAttributeOfType<DescriptionAttribute>();

            return attr != null ? attr.Description : en.ToString();
        }
    }
}