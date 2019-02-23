namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Reactive.Linq;

    public class COMService : IComService
    {
        readonly SerialPort _sp = new SerialPort();

        public event EventHandler<string> DataReceived;

        public event EventHandler<string> LineReceived;

        public event EventHandler<ConnectionState> ConnectionStateChanged;

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
            ConnectionStateChanged?.Invoke(this, _sp.IsOpen ? ConnectionState.Online : ConnectionState.Offline);
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
            var cnt = _sp.BytesToRead;
            byte[] buffer = new byte[cnt];
            if (!_sp.IsOpen)
            {
                throw new InvalidOperationException("Serial port is closed.");
            }
            _sp.Read(buffer, 0, cnt);

            var serialData = System.Text.Encoding.UTF8.GetString(buffer);

            Console.WriteLine("Incoming: {0}", serialData);

            var lines = (_buffer + serialData).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (lines.Any())
            {
                if (lines.Last() == string.Empty)
                {
                    _buffer = string.Empty;
                    Console.WriteLine("Buf: {0}", _buffer);
                    foreach (var line in lines)
                    {
                        OnLineReceived(line);
                    }
                }
                else
                {
                    _buffer = lines.Last();
                    Console.WriteLine("Buf: {0}", _buffer);
                    foreach (var line in lines.Take(lines.Length - 1))
                    {
                        OnLineReceived(line);
                    }
                }
            }

            OnDataReceived(serialData);
        }

        public void Disconnect()
        {
            if (!_sp.IsOpen)
            {
                return;
            }

            Observable.Start(
                () =>
                    {
                        _sp.DiscardInBuffer();
                        _sp.DiscardOutBuffer();
                        //Thread.Sleep(3000);
                        _sp.Close();

                        OnConnectionStateChanged();
                    }).Subscribe();

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
