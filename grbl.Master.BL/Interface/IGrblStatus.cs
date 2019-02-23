namespace grbl.Master.BL.Interface
{
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;

    public interface IGrblStatus
    {
        MachineState MachineState { get; set; }

        Position MachinePosition { get; set; }

        Position WorkPosition { get; set; }

        Position WorkOffset { get; set; }

        BufferState BufferState { get; set; }

        long LineNumber { get; set; }

        FeedAndSpeed FeedAndSpeed { get; set; }

        InputPinState InputPinState { get; set; }

        OverrideValues OverrideValues { get; set; }

        AccessoryState AccessoryState { get; set; }
    }
}