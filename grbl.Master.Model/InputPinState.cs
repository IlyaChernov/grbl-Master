namespace grbl.Master.Model
{
    public class InputPinState : NotifyPropertyChanged
    {
        public bool XLimitPin { get; set; }

        public bool YLimitPin { get; set; }

        public bool ZLimitPin { get; set; }

        public bool ProbePin { get; set; }

        public bool DoorPin { get; set; }

        public bool HoldPin { get; set; }

        public bool SoftResetPin { get; set; }

        public bool CycleStartPin { get; set; }
    }
}
