namespace grbl.Master.Model
{
    using grbl.Master.Service.Annotations;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class InputPinState : INotifyPropertyChanged
    {
        private bool _xLimitPin;
        private bool _yLimitPin;
        private bool _zLimitPin;
        private bool _probePin;
        private bool _doorPin;
        private bool _holdPin;
        private bool _softResetPin;
        private bool _cycleStartPin;



        public event PropertyChangedEventHandler PropertyChanged;

        public bool XLimitPin
        {
            get => _xLimitPin;
            set
            {
                _xLimitPin = value;
                OnPropertyChanged();
            }
        }

        public bool YLimitPin
        {
            get => _yLimitPin;
            set
            {
                _yLimitPin = value;
                OnPropertyChanged();
            }
        }

        public bool ZLimitPin
        {
            get => _zLimitPin;
            set
            {
                _zLimitPin = value;
                OnPropertyChanged();
            }
        }

        public bool ProbePin
        {
            get => _probePin;
            set
            {
                _probePin = value;
                OnPropertyChanged();
            }
        }

        public bool DoorPin
        {
            get => _doorPin;
            set
            {
                _doorPin = value;
                OnPropertyChanged();
            }
        }

        public bool HoldPin
        {
            get => _holdPin;
            set
            {
                _holdPin = value;
                OnPropertyChanged();
            }
        }

        public bool SoftResetPin
        {
            get => _softResetPin;
            set
            {
                _softResetPin = value;
                OnPropertyChanged();
            }
        }

        public bool CycleStartPin
        {
            get => _cycleStartPin;
            set
            {
                _cycleStartPin = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
