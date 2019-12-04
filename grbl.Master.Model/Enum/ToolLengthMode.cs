namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    public enum ToolLengthMode
    {
        [Description("Dynamic tool length offset")]
        G43_1,
        [Description("Cancelled")]
        G49
    }
}