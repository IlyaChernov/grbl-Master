namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    public enum ActivePlane
    {
        [Description("XY plane selection")]
        G17,
        [Description("XZ plane selection")]
        G18,
        [Description("YZ plane selection")]
        G19
    }
}