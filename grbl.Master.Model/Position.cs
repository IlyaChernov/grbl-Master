namespace grbl.Master.Model
{
    public class Position : NotifyPropertyChanged
    {
        public decimal X { get; set; }

        public decimal Y { get; set; }

        public decimal Z { get; set; }

        public static Position operator +(Position first, Position second)
        {
            return new Position { X = first.X + second.X, Y = first.Y + second.Y, Z = first.Z + second.Z };
        }

        public static Position operator -(Position first, Position second)
        {
            return new Position { X = first.X - second.X, Y = first.Y - second.Y, Z = first.Z - second.Z };
        }

        public void Update(Position newValue)
        {
            X = newValue.X;
            Y = newValue.Y;
            Z = newValue.Z;
        }
    }
}