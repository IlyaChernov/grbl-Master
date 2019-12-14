namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum DistanceMode
    {
        [Description("Absolute")]
        G90,
        [Description("Relative")]
        G91
    }
}