namespace grbl.Master.Service.Interface
{
    using System;
    using System.Collections.Generic;

    using grbl.Master.Service.Enum;

    public interface IComService
    {
        event EventHandler<string> LineReceived;

        event EventHandler<ConnectionState> ConnectionStateChanged;

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
