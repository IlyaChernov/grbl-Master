namespace grbl.Master.Model.Interface
{
    using System;
    using System.ComponentModel;

    using grbl.Master.Model;
    using grbl.Master.Model.Enum;

    using Meziantou.Framework.WPF.Collections;

    public interface IGrblStatusModel
    {
        event EventHandler<MachineState> MachineStateChanged;

        ConcurrentObservableCollection<string> Messages { get; }

        GrblSettings Settings { get; }

        string LastMessage { get; set; }

        MachineState MachineState { get; set; }

        MotionMode MotionMode { get; set; }

        CoordinateSystem CoordinateSystem { get; set; }

        ActivePlane ActivePlane { get; set; }

        DistanceMode DistanceMode { get; set; }

        ArcDistanceMode ArcDistanceMode { get; set; }

        FeedRateMode FeedRateMode { get; set; }

        UnitsMode UnitsMode { get; set; }

        CutterRadiusCompensation CutterRadiusCompensation { get; set; }

        ToolLengthMode ToolLengthMode { get; set; }

        ProgramMode ProgramMode { get; set; }

        SpindleState SpindleState { get; set; }

        CoolantState CoolantState { get; set; }

        Position MachinePosition { get; set; }

        Position WorkPosition { get; set; }

        decimal ToolLengthOffset { get; set; }

        bool ProbeState { get; set; }

        Position ProbePosition { get; set; }

        Position G54Position { get; set; }

        Position G55Position { get; set; }

        Position G56Position { get; set; }

        Position G57Position { get; set; }

        Position G58Position { get; set; }

        Position G59Position { get; set; }

        Position G28Position { get; set; }

        Position G30Position { get; set; }

        Position G92Position { get; set; }

        Position WorkOffset { get; set; }

        BufferState BufferState { get; set; }

        long LineNumber { get; set; }

        FeedAndSpeed FeedAndSpeed { get; set; }

        InputPinState InputPinState { get; set; }

        OverrideValues OverrideValues { get; set; }

        AccessoryState AccessoryState { get; set; }

        //void OnMachineStateChanged();

        event PropertyChangedEventHandler PropertyChanged;
    }
}