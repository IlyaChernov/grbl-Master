namespace grbl.Master.Service.Interface
{
    using System;
    using System.Collections.Generic;

    public interface IComService
    {
        event EventHandler<string> DataReceived;

        event EventHandler<string> LineReceived;

        event EventHandler ConnectionStateChanged;

        List<string> GetPortNames();

        void Connect(string portName, int baudRate);

        void Send(string data);

        void Disconnect();        

        bool IsConnected
        {
            get;
        }
    }
}
