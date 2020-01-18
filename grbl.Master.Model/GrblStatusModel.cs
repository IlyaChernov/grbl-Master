namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.Annotations;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class GrblStatusModel : INotifyPropertyChanged //, IGrblStatus
    {
        public event EventHandler<MachineState> MachineStateChanged;

        private MachineState _machineState;
        private MotionMode _motionMode;
        private CoordinateSystem _coordinateSystem;
        private ActivePlane _activePlane;
        private DistanceMode _distanceMode;
        private ArcDistanceMode _arcDistanceMode;
        private FeedRateMode _feedRateMode;
        private UnitsMode _unitsMode;
        private CutterRaduisCompensation _cutterRaduisCompensation;
        private ToolLengthMode _toolLengthMode;
        private ProgramMode _programMode;
        private SpindleState _spindleState;
        private CoolantState _coolantState;
        private Position _machinePosition = new Position();
        private Position _workPosition = new Position();

        private decimal _toolLengthOffset;
        private Position _probePosition = new Position();
        private bool _probeState;
        private Position _g54Position = new Position();
        private Position _g55Position = new Position();
        private Position _g56Position = new Position();
        private Position _g57Position = new Position();
        private Position _g58Position = new Position();
        private Position _g59Position = new Position();
        private Position _g28Position = new Position();
        private Position _g30Position = new Position();
        private Position _g92Position = new Position();

        private Position _workOffset = new Position();
        private BufferState _bufferState = new BufferState();
        private FeedAndSpeed _feedAndSpeed = new FeedAndSpeed();
        private InputPinState _inputPinState = new InputPinState();
        private OverrideValues _overrideValues = new OverrideValues();

        private long _lineNumber;

        public MachineState MachineState
        {
            get => _machineState;
            set
            {
                if (_machineState != value)
                {
                    _machineState = value;
                    OnPropertyChanged();
                    OnMachineStateChanged();
                }
            }
        }

        public MotionMode MotionMode
        {
            get => _motionMode;
            set
            {
                if (_motionMode != value)
                {
                    _motionMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public CoordinateSystem CoordinateSystem
        {
            get => _coordinateSystem;
            set
            {
                if (_coordinateSystem != value)
                {
                    _coordinateSystem = value;
                    OnPropertyChanged();
                }
            }
        }

        public ActivePlane ActivePlane
        {
            get => _activePlane;
            set
            {
                if (_activePlane != value)
                {
                    _activePlane = value;
                    OnPropertyChanged();
                }
            }
        }

        public DistanceMode DistanceMode
        {
            get => _distanceMode;
            set
            {
                if (_distanceMode != value)
                {
                    _distanceMode = value;
                    OnPropertyChanged();
                }
            }
        }
         
        public ArcDistanceMode ArcDistanceMode
        {
            get => _arcDistanceMode;
            set
            {
                if (_arcDistanceMode != value)
                {
                    _arcDistanceMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public FeedRateMode FeedRateMode
        {
            get => _feedRateMode;
            set
            {
                if (_feedRateMode != value)
                {
                    _feedRateMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public UnitsMode UnitsMode
        {
            get => _unitsMode;
            set
            {
                if (_unitsMode != value)
                {
                    _unitsMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public CutterRaduisCompensation CutterRaduisCompensation
        {
            get => _cutterRaduisCompensation;
            set
            {
                if (_cutterRaduisCompensation != value)
                {
                    _cutterRaduisCompensation = value;
                    OnPropertyChanged();
                }
            }
        }

        public ToolLengthMode ToolLengthMode
        {
            get => _toolLengthMode;
            set
            {
                if (_toolLengthMode != value)
                {
                    _toolLengthMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public ProgramMode ProgramMode
        {
            get => _programMode;
            set
            {
                if (_programMode != value)
                {
                    _programMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public SpindleState SpindleState
        {
            get => _spindleState;
            set
            {
                if (_spindleState != value)
                {
                    _spindleState = value;
                    OnPropertyChanged();
                }
            }
        }

        public CoolantState CoolantState
        {
            get => _coolantState;
            set
            {
                if (_coolantState != value)
                {
                    _coolantState = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual void OnMachineStateChanged()
        {
            MachineStateChanged?.Invoke(this, MachineState);
        }

        public Position MachinePosition
        {
            get => _machinePosition;
            set
            {
                _machinePosition = value;
                OnPropertyChanged();
            }
        }

        public Position WorkPosition
        {
            get => _workPosition;
            set
            {
                _workPosition = value;
                OnPropertyChanged();
            }
        }

        public decimal ToolLengthOffset
        {
            get => _toolLengthOffset;
            set
            {
                _toolLengthOffset = value;

                OnPropertyChanged();
            }
        }

        public bool ProbeState
        {
            get => _probeState;
            set
            {
                _probeState = value;
                OnPropertyChanged();
            }
        }

        public Position ProbePosition
        {
            get => _probePosition;
            set
            {
                _probePosition = value;
                OnPropertyChanged();
            }
        }

        public Position G54Position
        {
            get => _g54Position;
            set
            {
                _g54Position = value;
                OnPropertyChanged();
            }
        }

        public Position G55Position
        {
            get => _g55Position;
            set
            {
                _g55Position = value;
                OnPropertyChanged();
            }
        }

        public Position G56Position
        {
            get => _g56Position;
            set
            {
                _g56Position = value;
                OnPropertyChanged();
            }
        }

        public Position G57Position
        {
            get => _g57Position;
            set
            {
                _g57Position = value;
                OnPropertyChanged();
            }
        }

        public Position G58Position
        {
            get => _g58Position;
            set
            {
                _g58Position = value;
                OnPropertyChanged();
            }
        }

        public Position G59Position
        {
            get => _g59Position;
            set
            {
                _g59Position = value;
                OnPropertyChanged();
            }
        }

        public Position G28Position
        {
            get => _g28Position;
            set
            {
                _g28Position = value;
                OnPropertyChanged();
            }
        }

        public Position G30Position
        {
            get => _g30Position;
            set
            {
                _g30Position = value;
                OnPropertyChanged();
            }
        }

        public Position G92Position
        {
            get => _g92Position;
            set
            {
                _g92Position = value;
                OnPropertyChanged();
            }
        }

        public Position WorkOffset
        {
            get => _workOffset;
            set
            {
                _workOffset = value;
                OnPropertyChanged();
            }
        }

        public BufferState BufferState
        {
            get => _bufferState;
            set
            {
                _bufferState = value;
                OnPropertyChanged();
            }
        }

        public long LineNumber
        {
            get => _lineNumber;
            set
            {
                _lineNumber = value;
                OnPropertyChanged();
            }
        }

        public FeedAndSpeed FeedAndSpeed
        {
            get => _feedAndSpeed;
            set
            {
                _feedAndSpeed = value;
                OnPropertyChanged();
            }
        }

        public InputPinState InputPinState
        {
            get => _inputPinState;
            set
            {
                _inputPinState = value;
                OnPropertyChanged();
            }
        }

        public OverrideValues OverrideValues
        {
            get => _overrideValues;
            set
            {
                _overrideValues = value;
                OnPropertyChanged();
            }
        }

      

        //public AccessoryState AccessoryState
        //{
        //    get => _accessoryState;
        //    set
        //    {
        //        _accessoryState = value;
        //        OnPropertyChanged();
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
