namespace grbl.Master.UI.Converters
{
    using System;
    using System.Windows.Data;

    public class IntegerMask3ToDescription : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value as string switch
                {
                    "0" => "Enable WPos: Disable MPos:.",
                    "1" => "Enable MPos:. Disable WPos:.",
                    _ => "Enabled Buf: field appears with planner and serial RX available buffer."
                };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }
    }
}