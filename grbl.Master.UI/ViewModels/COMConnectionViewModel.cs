using Caliburn.Micro;
using grbl.Master.Communication;
using System;

namespace grbl.Master.UI.ViewModels
{
    public class COMConnectionViewModel : Screen
    {
        private ICOMService _comService;
        private BindableCollection<string> _comPorts = new BindableCollection<string>();
        private BindableCollection<string> _receivedData = new BindableCollection<string>();
        private string _selectedComPort;

        public COMConnectionViewModel(ICOMService comService)
        {
            _comService = comService;
            _comService.ConnectionStateChanged += _comService_ConnectionStateChanged;
            ReloadComPorts();
        }

        private void _comService_ConnectionStateChanged(object sender, System.EventArgs e)
        {
            NotifyOfPropertyChange(() => ConnectButtonCaption);
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

        public string SelectedComPort
        {
            get
            {
                return _selectedComPort;
            }
            set
            {
                _selectedComPort = value;
                NotifyOfPropertyChange(() => SelectedComPort);
                NotifyOfPropertyChange(() => CanConnect);
            }
        }

        public string ConnectButtonCaption => _comService.IsConnected ? "Disconnect" : "Connect";

        public void ReloadComPorts()
        {
            ComPorts = new BindableCollection<string>(_comService.GetPortNames());
        }

        public bool CanConnect => !string.IsNullOrWhiteSpace(SelectedComPort) || _comService.IsConnected;

        public void Connect()
        {
            if (_comService.IsConnected)
            {
                _comService.Disconnect();
            }
            else
            {
                _comService.Connect(SelectedComPort, 115200);
            }
        }
    }
}
