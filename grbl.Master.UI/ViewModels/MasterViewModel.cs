using Caliburn.Micro;
using grbl.Master.Communication;
using System;

namespace grbl.Master.UI.ViewModels
{
    public class MasterViewModel : Screen
    {
        private ICOMService _comService;
        private string _receivedData;
        private string _manualCommand;

        public MasterViewModel(ICOMService comService)
        {
            COMConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _comService.DataReceived += _comService_DataReceived;
            _comService.ConnectionStateChanged += _comService_ConnectionStateChanged;
        }

        private void _comService_ConnectionStateChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => CanSendManualCommand);
            NotifyOfPropertyChange(() => CanSendEnterCommand);

        }

        public COMConnectionViewModel COMConnectionViewModel
        {
            get;
            private set;
        }

        public string ReceivedData
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

        public string ManualCommand
        {
            get
            {
                return _manualCommand;
            }
            set
            {
                _manualCommand = value;
                NotifyOfPropertyChange(() => ManualCommand);
                NotifyOfPropertyChange(() => CanSendManualCommand);
            }
        }

        public bool CanSendManualCommand => !string.IsNullOrWhiteSpace(ManualCommand) && _comService.IsConnected;

        public void SendManualCommand()
        {
            _comService.Send(ManualCommand);
        }

        public bool CanSendEnterCommand => _comService.IsConnected;

        public void SendEnterCommand()
        {
            SendManualCommand();
        }

        private void _comService_DataReceived(object sender, string e)
        {
            ReceivedData += e;
        }
    }
}
