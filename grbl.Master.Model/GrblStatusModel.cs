namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.Annotations;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class GrblStatusModel : INotifyPropertyChanged //, IGrblStatus
    {
        private MachineState _machineState;
        private Position _machinePosition = new Position();
        private Position _workPosition = new Position();
        private Position _workOffset = new Position();
        private BufferState _bufferState = new BufferState();
        private FeedAndSpeed _feedAndSpeed = new FeedAndSpeed();
        private InputPinState _inputPinState = new InputPinState();
        private OverrideValues _overrideValues = new OverrideValues();
        private AccessoryState _accessoryState = new AccessoryState();

        private long _lineNumber;

        public MachineState MachineState
        {
            get => _machineState;
            set
            {
                _machineState = value;
                OnPropertyChanged();
            }
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

        public AccessoryState AccessoryState
        {
            get => _accessoryState;
            set
            {
                _accessoryState = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
