namespace grbl.Master.Model
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using grbl.Master.Model.Enum;

    using JetBrains.Annotations;

    public class AccessoryState : INotifyPropertyChanged
    {
        private bool _mist;
        private bool _flood;
        private SpindleState _spindle;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Mist
        {
            get => _mist;
            set
            {
                _mist = value;
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

        public SpindleState Spindle
        {
            get => _spindle;
            set
            {
                _spindle = value;
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
