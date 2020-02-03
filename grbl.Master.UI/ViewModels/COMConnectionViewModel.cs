using Caliburn.Micro;
using System.Collections.Generic;

namespace grbl.Master.UI.ViewModels
{
    using System.Windows;

    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;

    public class COMConnectionViewModel : Screen
    {
        private readonly IComService _comService;
        private BindableCollection<string> _comPorts = new BindableCollection<string>();
        private BindableCollection<string> _receivedData = new BindableCollection<string>();
        private List<int> _baudRates = new List<int> { 9600, 115200 };

        public COMConnectionViewModel(IComService comService)
        {
            _comService = comService;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            ReloadComPorts();
        }

        private void  ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            NotifyOfPropertyChange(() => ConnectButtonCaption);
            NotifyOfPropertyChange(()=> CanChangePortBaud);
        }

        public BindableCollection<string> ComPorts
        {
            get => _comPorts;
            set
            {
                _comPorts = value;
                NotifyOfPropertyChange(() => ComPorts);
                NotifyOfPropertyChange(() => SelectedComPort);
            }
        }

        public List<int> BaudRates
        {
            get => _baudRates;
            set
            {
                _baudRates = value;
                NotifyOfPropertyChange(() => BaudRates);
                NotifyOfPropertyChange(() => SelectedBaudRate);
            }
        }

        public BindableCollection<string> ReceivedData
        {
            get => _receivedData;
            set
            {
                _receivedData = value;
                NotifyOfPropertyChange(() => ReceivedData);
            }
        }

        public int SelectedBaudRate
        {
            get => Model.Properties.Settings.Default.SelectedBaudRate;
            set
            {
                Model.Properties.Settings.Default.SelectedBaudRate = value;
                Model.Properties.Settings.Default.Save();
                Model.Properties.Settings.Default.Reload();

                NotifyOfPropertyChange(() => SelectedBaudRate);
                NotifyOfPropertyChange(() => CanConnect);
            }
        }

        public string SelectedComPort
        {
            get => Model.Properties.Settings.Default.SelectedComPort;
            set
            {
                Model.Properties.Settings.Default.SelectedComPort = value;
                Model.Properties.Settings.Default.Save();
                Model.Properties.Settings.Default.Reload();

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
            try
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
            catch
            {
                MessageBox.Show("Something goes wrong while changing connection state");
            }
        }
    }
}
