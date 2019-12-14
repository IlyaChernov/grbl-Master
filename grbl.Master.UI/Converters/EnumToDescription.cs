namespace grbl.Master.UI.Converters
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Data;

    using grbl.Master.Service.Enum;

    public class EnumToDescription : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return DependencyProperty.UnsetValue;

            return GetDescription((Enum)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            foreach (Enum one in Enum.GetValues(targetType))
            {
                if (value.ToString() == GetDescription(one))
                    return one;
            }
            return null;
        }

        public static string GetDescription(Enum en)
        {

            var attr = en.GetAttributeOfType<DescriptionAttribute>();

            if (attr != null)
            {
                return attr.Description;
            }

            return en.ToString();
        }
    }
}