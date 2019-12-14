namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Attribute;
    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum CoordinateSystem
    {
        [ChangeCommand("G54")]
        G54,
        [ChangeCommand("G55")]
        G55,
        [ChangeCommand("G56")]
        G56,
        [ChangeCommand("G57")]
        G57,
        [ChangeCommand("G58")]
        G58,
        [ChangeCommand("G59")]
        G59
    }
}