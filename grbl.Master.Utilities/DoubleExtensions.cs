namespace grbl.Master.Utilities
{
    public static class DoubleExtensions
    {
        public static string ToGrblString(this double value)
        {
            return value.ToString().ToGrblString();
        }
    }
}
