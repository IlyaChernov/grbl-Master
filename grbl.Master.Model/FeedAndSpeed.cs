namespace grbl.Master.Model
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    public class FeedAndSpeed : INotifyPropertyChanged
    {
        private long _feed;

        private long _speed;

        private long _tool;

        public event PropertyChangedEventHandler PropertyChanged;

        public long FeedRate
        {
            get => _feed;
            set
            {
                _feed = value;
                OnPropertyChanged();
            }
        }

        public long ToolNumber
        {
            get => _tool;
            set
            {
                _tool = value;
                OnPropertyChanged();
            }
        }

        public long SpindleSpeed
        {
            get => _speed;
            set
            {
                _speed = value;
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
