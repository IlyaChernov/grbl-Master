﻿namespace grbl.Master.BL.Interface
{
    using System;    
    using grbl.Master.Model;

    public interface IGrblStatus
    {
        event EventHandler StatusReceived;

        void StartRequesting(TimeSpan interval);
        void StopRequesting();  

        bool IsRunning
        {
            get;           
        }

        GrblStatusModel GrblStatusModel { get; set; } 
    }
}