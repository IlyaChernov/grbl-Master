using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace grbl.Master.Communication
{
    public class COMService : ICOMService
    {
        SerialPort sp = new SerialPort();

        public event EventHandler<string> DataReceived;

        public virtual void OnDataReceived(string e)
        {
            DataReceived?.Invoke(this, e);
        }

        public bool Connect(string portName, int baudRate)
        {
            if (sp.IsOpen) return false;
            sp.PortName = portName;
            sp.BaudRate = baudRate;
            sp.ReadTimeout = 5000;
            sp.Open();
            ResetBoard();
            ReadFromCom();
            return true;
        }

        private async void ReadFromCom()
        {
            if (sp.IsOpen)
            {
                var buffer = new byte[256];
                var lenght = await sp.BaseStream.ReadAsync(buffer, 0, 256);

                OnDataReceived(Encoding.ASCII.GetString(buffer));
            }

            ReadFromCom();
        }

        public List<string> GetPortNames()
        {
            return SerialPort.GetPortNames().ToList();
        }

        public void ResetBoard()
        {
            if (sp.IsOpen)
            {
                sp.DtrEnable = true;
                sp.DtrEnable = false;
            }
        }
    }
}
