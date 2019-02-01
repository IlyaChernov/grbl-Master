using System;
using System.Collections.Generic;

namespace grbl.Master.Communication
{    
    public interface IComService
    {
        event EventHandler<string> DataReceived;

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
