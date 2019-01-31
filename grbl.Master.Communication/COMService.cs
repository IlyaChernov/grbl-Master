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

        public event EventHandler ConnectionStateChanged;

        public virtual void OnDataReceived(string e)
        {
            DataReceived?.Invoke(this, e);
        }

        public virtual void OnConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsConnected => sp.IsOpen;

        public void Connect(string portName, int baudRate)
        {
            if (sp.IsOpen)
            {
                return;
            }

            sp.PortName = portName;
            sp.BaudRate = baudRate;
            sp.ReadTimeout = 5000;
            sp.Open();
            ResetBoard();
            sp.DataReceived += Sp_DataReceived;
            //ReadFromCom();
            OnConnectionStateChanged();
        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            OnDataReceived(sp.ReadExisting());
        }

        public void Disconnect()
        {
            if (!sp.IsOpen)
            {
                return;
            }
            //sp.BaseStream.Close();
            sp.Close();
            OnConnectionStateChanged();
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

        public void Send(string data)
        {
            if (sp.IsOpen)
            {
                sp.WriteLine(data);
            }
        }
    }
}
