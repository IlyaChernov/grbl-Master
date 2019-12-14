namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum ArcDistanceMode
    {
        [Description("Incremental distance mode")]
        G91_1
    }
}