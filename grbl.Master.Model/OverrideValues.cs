namespace grbl.Master.Model
{
    public class OverrideValues : NotifyPropertyChanged
    {
        public int Feed { get; set; }

        public int Rapid { get; set; }

        public int Spindle { get; set; }
    }
}
