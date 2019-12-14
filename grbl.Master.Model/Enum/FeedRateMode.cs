namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum FeedRateMode
    {
        [Description("Time to finish")]
        G93,
        [Description("Units per minute")]
        G94
    }
}