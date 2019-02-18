namespace grbl.Master.Model
{
    using grbl.Master.Service.Annotations;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Position : INotifyPropertyChanged
    {
        private decimal _x;
        private decimal _y;
        private decimal _z;

        public decimal X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged();
            }
        }

        public decimal Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged();
            }
        }

        public decimal Z
        {
            get => _z;
            set
            {
                _z = value;
                OnPropertyChanged();
            }
        }

        public static Position operator +(Position first, Position second)
        {
            return new Position { _x = first._x + second._x, _y = first._y + second._y, _z = first._z + second._z };
        }

        public static Position operator -(Position first, Position second)
        {
            return new Position { _x = first._x - second._x, _y = first._y - second._y, _z = first._z - second._z };
        }

        public void Update(Position newValue)
        {
            X = newValue.X;
            Y = newValue.Y;
            Z = newValue.Z;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
