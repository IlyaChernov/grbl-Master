namespace grbl.Master.Utilities
{
    public static class DecimalExtensions
    {
        public static string ToGrblString(this decimal value)
        {
            return ((double)value).ToGrblString();
        }
    }
}
