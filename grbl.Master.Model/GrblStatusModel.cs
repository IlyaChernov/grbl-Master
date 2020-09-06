namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;
    using Meziantou.Framework.WPF.Collections;
    using System;
    using System.Linq;

    using grbl.Master.Model.Interface;

    public class GrblStatusModel : NotifyPropertyChanged, IGrblStatusModel
    {
        public event EventHandler<MachineState> MachineStateChanged;

        private MachineState _machineState;
        private string _lastMessage;

        public ConcurrentObservableCollection<string> Messages { get; } = new ConcurrentObservableCollection<string>();

        public GrblSettings Settings { get; } = new GrblSettings();

        public string LastMessage
        {
            get => _lastMessage;
            set
            {
                if (_lastMessage != value)
                {
                    _lastMessage = value;
                    Messages.Insert(0, value);
                    if (Messages.Count > 10)
                    {
                        Messages.Remove(Messages.Last());
                    }
                }
            }
        }

        public MachineState MachineState
        {
            get => _machineState;
            set
            {
                if (_machineState != value)
                {
                    _machineState = value;
                    OnMachineStateChanged();
                }
            }
        }

        public MotionMode MotionMode { get; set; }

        public CoordinateSystem CoordinateSystem { get; set; }

        public ActivePlane ActivePlane { get; set; }

        public DistanceMode DistanceMode { get; set; }

        public ArcDistanceMode ArcDistanceMode { get; set; }

        public FeedRateMode FeedRateMode { get; set; }

        public UnitsMode UnitsMode { get; set; }

        public CutterRadiusCompensation CutterRadiusCompensation { get; set; }

        public ToolLengthMode ToolLengthMode { get; set; }

        public ProgramMode ProgramMode { get; set; }

        public SpindleState SpindleState { get; set; }

        public CoolantState CoolantState { get; set; }

        private void OnMachineStateChanged()
        {
            MachineStateChanged?.Invoke(this, MachineState);
        }

        public Position MachinePosition { get; set; } = new Position();

        public Position WorkPosition { get; set; } = new Position();

        public decimal ToolLengthOffset { get; set; }

        public bool ProbeState { get; set; }

        public Position ProbePosition { get; set; } = new Position();

        public Position G54Position { get; set; } = new Position();

        public Position G55Position { get; set; } = new Position();

        public Position G56Position { get; set; } = new Position();

        public Position G57Position { get; set; } = new Position();

        public Position G58Position { get; set; } = new Position();

        public Position G59Position { get; set; } = new Position();

        public Position G28Position { get; set; } = new Position();

        public Position G30Position { get; set; } = new Position();

        public Position G92Position { get; set; } = new Position();

        public Position WorkOffset { get; set; } = new Position();

        public BufferState BufferState { get; set; } = new BufferState();

        public long LineNumber { get; set; }

        public FeedAndSpeed FeedAndSpeed { get; set; } = new FeedAndSpeed();

        public InputPinState InputPinState { get; set; } = new  InputPinState();

        public OverrideValues OverrideValues { get; set; } = new OverrideValues();

        public AccessoryState AccessoryState { get; set; } = new AccessoryState();
    }
}
