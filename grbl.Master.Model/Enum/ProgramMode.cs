namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum ProgramMode
    {
        [Description("M0, Pause")]
        M0,
        [Description("M1, Stop, Ignored")]
        M1,
        [Description("M2, Stop and reset")]
        M2,
        [Description("M30, Stop and reset")]
        M30

    }
}