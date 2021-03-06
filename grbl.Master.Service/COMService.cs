﻿namespace grbl.Master.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text;

    using grbl.Master.Common.Enum;
    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Utilities;

    public class COMService : IComService
    {
        private readonly SerialPort _sp = new SerialPort();

        public event EventHandler<string> LineReceived;

        public event EventHandler<ConnectionState> ConnectionStateChanged;

        private string _buffer = "";

        private readonly Subject<Unit> _stopSubject = new Subject<Unit>();

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
            ConnectionStateChanged?.Invoke(this, IsConnected ? ConnectionState.Online : ConnectionState.Offline);
        }

        public bool IsConnected => _sp.IsOpen;

        public void Connect(string portName, int baudRate)
        {
            if (IsConnected)
            {
                return;
            }

            _sp.PortName = portName;
            _sp.BaudRate = baudRate;
            _sp.ReadTimeout = 5000;
            _sp.Encoding = Encoding.GetEncoding(28591);
            _sp.Open();
            _sp.DiscardInBuffer();
            _sp.DiscardOutBuffer();
            ResetBoard();
            _sp.ReceivedBytesThreshold = 2;
            _sp.DataReceived += SpDataReceived;

            OnConnectionStateChanged();

            PortMonitoring();
        }

        private void PortMonitoring()
        {
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)).TakeUntil(_stopSubject).Subscribe(
                l =>
                    {
                        var ports = GetPortNames();
                        if (!IsConnected || ports.All(x => x.ToLower() != _sp.PortName.ToLower()))
                        {
                            Disconnect();
                        }
                    });
        }

        private void SpDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_sp.IsOpen)
            {
                var cnt = _sp.BytesToRead;
                var buffer = new byte[cnt];
                if (!IsConnected)
                {
                    throw new InvalidOperationException("Serial port is closed.");
                }

                _sp.Read(buffer, 0, cnt);

                var serialData = Encoding.UTF8.GetString(buffer);

                var lines = (_buffer + serialData).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

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
            }
        }

        public void SendImmediate(string data)
        {
            if (IsConnected)
            {
                _sp.Write(data.RemoveSpace());
            }
        }

        public void Disconnect()
        {
            _stopSubject.OnNext(Unit.Default);

            Observable.Start(
                () =>
                    {
                        if (IsConnected)
                        {
                            _sp.DiscardInBuffer();
                            _sp.DiscardOutBuffer();
                            _sp.Close();
                        }

                        _buffer = string.Empty;
                        OnConnectionStateChanged();
                    }).Subscribe();
        }

        public List<string> GetPortNames()
        {
            return SerialPort.GetPortNames().ToList();
        }

        public void ResetBoard()
        {
            if (IsConnected)
            {
                _sp.DtrEnable = true;
                _sp.DtrEnable = false;
            }
        }

        public void Send(string data)
        {
            if (IsConnected)
            {
                try
                {
                    _sp.WriteLine(data.RemoveSpace());
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }
            }
        }
    }
}
