namespace grbl.Master.Utilities
{
    using System.Globalization;

    public static class DoubleExtensions
    {
        public static string ToGrblString(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture).ToGrblString();
        }
    }
}
