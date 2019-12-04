namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    public enum UnitsMode
    {
        [Description("Inch")]
        G20,
        [Description("mm")]
        G21
    }
}