namespace grbl.Master.Model.GCode
{
    using System.Collections.Generic;
    using System.Windows.Media.Media3D;

    public class GCodeFrame
    {
        public MovementType MovementType { get; set; }

        public List<Point3D> Points { get; set; }
    }
}
