using System;
using System.Collections.Generic;

namespace grbl.Master.Communication
{    
    public interface ICOMService
    {
        event EventHandler<string> DataReceived;

        event EventHandler ConnectionStateChanged;

        List<string> GetPortNames();

        void Connect(string portName, int baudRate);

        void Disconnect();

        void ResetBoard();

        bool IsConnected
        {
            get;
        }
    }
}
