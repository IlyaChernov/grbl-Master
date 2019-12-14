namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum SpindleState
    {
        [Description("Clockwise")]
        M3,
        [Description("Counterclockwise")]
        M4,
        [Description("Stop")]
        M5
    }
}