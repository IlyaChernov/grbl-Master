// ReSharper disable UnusedMember.Global
namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Converters;

    [TypeConverter(typeof(EnumToDescription))]
    public enum ActivePlane
    {
        [Description("XY plane")]
        G17,
        [Description("XZ plane")]
        G18,
        [Description("YZ plane")]
        G19
    }
}