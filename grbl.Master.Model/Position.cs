namespace grbl.Master.Model
{
    public class Position : NotifyPropertyChanged
    {
        //Linear
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Z { get; set; }

        //Rotational
        public decimal A { get; set; }
        public decimal B { get; set; }
        public decimal C { get; set; }

        public static Position operator +(Position first, Position second)
        {
            return new Position
            {
                X = first.X + second.X,
                Y = first.Y + second.Y,
                Z = first.Z + second.Z,

                A = first.A + second.A,
                B = first.B + second.B,
                C = first.C + second.C
            };
        }

        public static Position operator -(Position first, Position second)
        {
            return new Position
            {
                X = first.X - second.X,
                Y = first.Y - second.Y,
                Z = first.Z - second.Z,

                A = first.A - second.A,
                B = first.B - second.B,
                C = first.C - second.C
            };
        }

        public void Update(Position newValue)
        {
            if (X != newValue.X)
                X = newValue.X;
            if (Y != newValue.Y)
                Y = newValue.Y;
            if (Z != newValue.Z)
                Z = newValue.Z;

            if (A != newValue.A)
                A = newValue.A;
            if (B != newValue.B)
                B = newValue.B;
            if (C != newValue.C)
                C = newValue.C;
        }
    }
}