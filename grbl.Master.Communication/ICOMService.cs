using System.Collections.Generic;

namespace grbl.Master.Communication
{
    public interface ICOMService
    {
        List<string> GetPortNames();

        bool Connect(string portName, int baudRate);

        void ResetBoard();
    }
}
