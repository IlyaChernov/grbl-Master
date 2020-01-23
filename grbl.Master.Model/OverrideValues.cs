namespace grbl.Master.Model
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    public class OverrideValues : INotifyPropertyChanged
    {
        private int _feed;
        private int _rapid;
        private int _spindle;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Feed
        {
            get => _feed;
            set
            {
                _feed = value;
                OnPropertyChanged();
            }
        }

        public int Rapid
        {
            get => _rapid;
            set
            {
                _rapid = value;
                OnPropertyChanged();
            }
        }

        public int Spindle
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
