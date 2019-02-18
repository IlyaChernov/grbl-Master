namespace grbl.Master.Model
{
    using grbl.Master.Service.Annotations;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class FeedAndSpeed : INotifyPropertyChanged
    {
        private long _feed;

        private long _speed;

        public event PropertyChangedEventHandler PropertyChanged;

        public long Feed
        {
            get => _feed;
            set
            {
                _feed = value;
                OnPropertyChanged();
            }
        }

        public long Speed
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
