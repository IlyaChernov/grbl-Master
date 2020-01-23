namespace grbl.Master.BL.Interface
{
    using System;    
    using grbl.Master.Model;

    public interface IGrblStatus
    {
        void StartRequesting(TimeSpan positionsInterval, TimeSpan gStateInterval,TimeSpan offsetsInterval);
        void InitialRequest();
        void StopRequesting();  

        //bool IsRunning
        //{
        //    get;           
        //}

        GrblStatusModel GrblStatusModel { get; set; } 
    }
}
