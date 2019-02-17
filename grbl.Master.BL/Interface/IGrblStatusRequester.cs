namespace grbl.Master.BL.Interface
{
    using System;

    public interface IGrblStatusRequester
    {
        void StartRequesting(TimeSpan interval);
        void StopRequesting();

        bool IsRunning
        {
            get;
            set;
        }
    }
}
