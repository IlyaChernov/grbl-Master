namespace grbl.Master.Service.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;

    using grbl.Master.Service.Interface;

    public class COMService : IComService
    {
        readonly SerialPort _sp = new SerialPort();

        public event EventHandler<string> DataReceived;

        public event EventHandler<string> LineReceived;

        public event EventHandler ConnectionStateChanged;

        private string _buffer = "";

        public virtual void OnDataReceived(string e)
        {
            DataReceived?.Invoke(this, e);
        }

        public virtual void OnLineReceived(string e)
        {
            if (e == string.Empty)
            {
                return;
            }

            LineReceived?.Invoke(this, e);
        }

        public virtual void OnConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsConnected => _sp.IsOpen;

        public void Connect(string portName, int baudRate)
        {
            if (_sp.IsOpen)
            {
                return;
            }

            _sp.PortName = portName;
            _sp.BaudRate = baudRate;
            _sp.ReadTimeout = 5000;
            _sp.Open();
            ResetBoard();
            _sp.DataReceived += SpDataReceived;
            OnConnectionStateChanged();
        }

        private void SpDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine(_buffer);

            var serialData = _sp.ReadExisting();

            var lines = (_buffer + serialData).Split(new[] { "\r\n" }, StringSplitOptions.None);

            if (lines.Any())
            {
                if (lines.Last() == string.Empty)
                {
                    _buffer = string.Empty;
                    foreach (var line in lines)
                    {
                        OnLineReceived(line);
                    }
                }
                else
                {
                    _buffer = lines.Last();
                    foreach (var line in lines.Take(lines.Length - 1))
                    {
                        OnLineReceived(line);
                    }
                }
            }

            OnDataReceived(_sp.ReadExisting());
        }

        public void Disconnect()
        {
            if (!_sp.IsOpen)
            {
                return;
            }
            _sp.Close();
            OnConnectionStateChanged();
        }

        public List<string> GetPortNames()
        {
            return SerialPort.GetPortNames().ToList();
        }

        public void ResetBoard()
        {
            if (_sp.IsOpen)
            {
                _sp.DtrEnable = true;
                _sp.DtrEnable = false;
            }
        }

        public void Send(string data)
        {
            if (_sp.IsOpen)
            {
                _sp.WriteLine(data);
            }
        }
    }
}
