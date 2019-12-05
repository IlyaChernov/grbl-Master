namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    public enum DistanceMode
    {
        [Description("Absolute")]
        G90,
        [Description("Relative")]
        G91
    }
}