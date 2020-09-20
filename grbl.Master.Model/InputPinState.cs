namespace grbl.Master.Model
{
    using System;

    public class InputPinState : NotifyPropertyChanged
    {
        public DateTime UpdateDateTime { get; set; }

        public bool XLimitPin { get; set; }

        public bool YLimitPin { get; set; }

        public bool ZLimitPin { get; set; }

        public bool ProbePin { get; set; }

        public bool DoorPin { get; set; }

        public bool HoldPin { get; set; }

        public bool SoftResetPin { get; set; }

        public bool CycleStartPin { get; set; }

        public void Reset()
        {
            XLimitPin = YLimitPin = ZLimitPin = ProbePin = DoorPin = HoldPin = SoftResetPin = CycleStartPin = false;
            UpdateDateTime = DateTime.Now;
        }
    }
}
