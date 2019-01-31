﻿using Caliburn.Micro;
using grbl.Master.Communication;
using System.Collections.Generic;

namespace grbl.Master.UI.ViewModels
{
    public class COMConnectionViewModel : Screen
    {
        private ICOMService _comService;
        private BindableCollection<string> _comPorts = new BindableCollection<string>();
        private BindableCollection<string> _receivedData = new BindableCollection<string>();
        private List<int> _baudRates = new List<int> { 9600, 115200 };

        public COMConnectionViewModel(ICOMService comService)
        {
            _comService = comService;
            _comService.ConnectionStateChanged += _comService_ConnectionStateChanged;
            ReloadComPorts();
        }

        private void _comService_ConnectionStateChanged(object sender, System.EventArgs e)
        {
            NotifyOfPropertyChange(() => ConnectButtonCaption);
            NotifyOfPropertyChange(()=> CanChangePortBaud);
        }

        public BindableCollection<string> ComPorts
        {
            get
            {
                return _comPorts;
            }
            set
            {
                _comPorts = value;
                NotifyOfPropertyChange(() => ComPorts);
                NotifyOfPropertyChange(() => SelectedComPort);
            }
        }

        public List<int> BaudRates
        {
            get
            {
                return _baudRates;
            }
            set
            {
                _baudRates = value;
                NotifyOfPropertyChange(() => BaudRates);
                NotifyOfPropertyChange(() => SelectedBaudRate);
            }
        }

        public BindableCollection<string> ReceivedData
        {
            get
            {
                return _receivedData;
            }
            set
            {
                _receivedData = value;
                NotifyOfPropertyChange(() => ReceivedData);
            }
        }

        public int SelectedBaudRate
        {
            get
            {
                return Properties.Settings.Default.SelectedBaudRate;
            }
            set
            {
                Properties.Settings.Default.SelectedBaudRate = value;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                NotifyOfPropertyChange(() => SelectedBaudRate);
                NotifyOfPropertyChange(() => CanConnect);
            }
        }

        public string SelectedComPort
        {
            get
            {
                return Properties.Settings.Default.SelectedComPort;
            }
            set
            {
                Properties.Settings.Default.SelectedComPort = value;                
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                NotifyOfPropertyChange(() => SelectedComPort);
                NotifyOfPropertyChange(() => CanConnect);
            }
        }

        public string ConnectButtonCaption => _comService.IsConnected ? "Disconnect" : "Connect";

        public void ReloadComPorts()
        {
            ComPorts = new BindableCollection<string>(_comService.GetPortNames());
        }

        public bool CanChangePortBaud => !_comService.IsConnected;

        public bool CanConnect => (!string.IsNullOrWhiteSpace(SelectedComPort) && SelectedBaudRate > 0) || _comService.IsConnected;

        public void Connect()
        {
            if (_comService.IsConnected)
            {
                _comService.Disconnect();
            }
            else
            {
                _comService.Connect(SelectedComPort, SelectedBaudRate);
            }
        }
    }
}
