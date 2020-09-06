namespace grbl.Master.Common.Interfaces.BL
{
    using System;

    using grbl.Master.Model.Interface;

    public interface IGrblStatus
    {
        void StartRequesting(TimeSpan positionsInterval, TimeSpan gStateInterval,TimeSpan offsetsInterval);
        void InitialRequest();
        void StopRequesting();

        //bool IsRunning
        //{
        //    get;           
        //}

        IGrblStatusModel GrblStatusModel { get; } 
    }
}
