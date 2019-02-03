namespace grbl.Master.Service.Interface
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
