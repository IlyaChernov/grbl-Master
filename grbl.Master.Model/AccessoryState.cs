namespace grbl.Master.Model
{
    using grbl.Master.Service.Annotations;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class AccessoryState : INotifyPropertyChanged
    {
        private bool _spindleClockWise;
        private bool _spindleCounterClockWise;
        private bool _flood;
        private bool _mist;



        public event PropertyChangedEventHandler PropertyChanged;

        public bool SpindleCw
        {
            get => _spindleClockWise;
            set
            {
                if (value)
                {
                    SpindleCcw = false;
                }

                _spindleClockWise = value;
                OnPropertyChanged();
            }
        }

        public bool SpindleCcw
        {
            get => _spindleCounterClockWise;
            set
            {
                if (value)
                {
                    SpindleCw = false;
                }

                _spindleCounterClockWise = value;
                OnPropertyChanged();
            }
        }

        public bool Flood
        {
            get => _flood;
            set
            {
                _flood = value;
                OnPropertyChanged();
            }
        }

        public bool Mist
        {
            get => _mist;
            set
            {
                _mist = value;
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
