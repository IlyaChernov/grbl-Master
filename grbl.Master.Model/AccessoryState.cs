namespace grbl.Master.Model
{

    using grbl.Master.Model.Enum;

    public class AccessoryState : NotifyPropertyChanged
    {
        public bool Mist { get; set; }

        public bool Flood { get; set; }

        public SpindleState Spindle { get; set; }
    }
}
