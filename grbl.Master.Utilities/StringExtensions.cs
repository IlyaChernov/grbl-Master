namespace grbl.Master.Utilities
{
    public static class StringExtensions
    {
        public static string ToGrblString(this string value)
        {
            return value.Replace(',', '.');
        }
    }
}
