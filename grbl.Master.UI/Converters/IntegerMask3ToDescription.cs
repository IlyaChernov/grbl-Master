namespace grbl.Master.UI.Converters
{
    using System;
    using System.Windows.Data;

    public class IntegerMask3ToDescription : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value as string)
            {
                case "0": return "Enable WPos: Disable MPos:.";
                case "1": return "Enable MPos:. Disable WPos:.";
                default: return "Enabled Buf: field appears with planner and serial RX available buffer.";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }
    }
}