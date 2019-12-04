namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    public enum FeedRateMode
    {
        [Description("Time to finish")]
        G93,
        [Description("Units per minute")]
        G94
    }
}