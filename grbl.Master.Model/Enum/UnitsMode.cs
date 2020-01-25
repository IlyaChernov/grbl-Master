// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum UnitsMode
    {
        [Description("Inch")]
        G20,
        [Description("mm")]
        G21
    }
}