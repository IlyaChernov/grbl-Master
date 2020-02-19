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
            if (X != newValue.X)
                X = newValue.X;
            if (Y != newValue.Y)
                Y = newValue.Y;
            if (Z != newValue.Z)
                Z = newValue.Z;
        }
    }
}