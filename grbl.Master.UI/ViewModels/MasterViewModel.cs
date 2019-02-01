using Caliburn.Micro;
using grbl.Master.Communication;
using System;

namespace grbl.Master.UI.ViewModels
{
    public class MasterViewModel : Screen
    {
        private readonly IComService _comService;
        private string _receivedData;
        private string _manualCommand;

        public MasterViewModel(IComService comService)
        {
            this.ComConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _comService.DataReceived += this.ComServiceDataReceived;
            _comService.ConnectionStateChanged += this.ComServiceConnectionStateChanged;
        }

        private void ComServiceConnectionStateChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => CanSendManualCommand);
            NotifyOfPropertyChange(() => CanSendEnterCommand);

        }

        public COMConnectionViewModel ComConnectionViewModel
        {
            get;
        }

        public string ReceivedData
        {
            get => _receivedData;
            set
            {
                _receivedData = value;
                NotifyOfPropertyChange(() => ReceivedData);
            }
        }

        public string ManualCommand
        {
            get => _manualCommand;
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

        private void ComServiceDataReceived(object sender, string e)
        {
            ReceivedData += e;
        }
    }
}
