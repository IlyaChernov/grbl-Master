namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

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