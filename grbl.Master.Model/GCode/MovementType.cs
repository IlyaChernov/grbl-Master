namespace grbl.Master.Model.GCode
{
    using grbl.Master.Model.Attribute;

    public enum MovementType
    {
        [Regex("([Gg]0?0)(?![1-9])")]
        Rapid,
        [Regex("([Gg]0?1)(?![02-9])")]
        LinearFeed,
        [Regex("([Gg]0?2)(?![013-9])")]
        ArcCW,
        [Regex("([Gg]0?3)(?![0-24-9])")]
        ArcCCW,
        Unsupported
    }
}