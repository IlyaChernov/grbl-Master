namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum CoolantState
    {
        [Description("Mist")]
        M7,
        [Description("Flood")]
        M8,
        [Description("Off")]
        M9
    }
}