namespace grbl.Master.Model
{
    using grbl.Master.Service.Annotations;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class BufferState : INotifyPropertyChanged
    {
        private int _availableBlocks;

        private int _availableBytes;

        public event PropertyChangedEventHandler PropertyChanged;

        public int AvailableBlocks
        {
            get => _availableBlocks;
            set
            {
                _availableBlocks = value;
                OnPropertyChanged();
            }
        }

        public int AvailableBytes
        {
            get => _availableBytes;
            set
            {
                _availableBytes = value;
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
