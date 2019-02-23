namespace grbl.Master.BL.Interface
{
    using System;

    public interface IGrblStatusProcessor
    {
        event EventHandler StatusReceived;
    }
}
