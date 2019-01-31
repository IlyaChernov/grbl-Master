using Caliburn.Micro;
using grbl.Master.Communication;
using System;

namespace grbl.Master.UI.ViewModels
{
    public class MasterViewModel : Screen
    {
        private ICOMService _comService;
        private BindableCollection<string> _receivedData = new BindableCollection<string>();
        private string _manualCommand;

        public MasterViewModel(ICOMService comService)
        {
            COMConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _comService.DataReceived += _comService_DataReceived;
        }

        public COMConnectionViewModel COMConnectionViewModel
        {
            get;
            private set;
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

        public bool CanSendManualCommand => !string.IsNullOrWhiteSpace(ManualCommand) || _comService.IsConnected;

        public void SendManualCommand()
        {
            _comService.Send(ManualCommand);
        }

        private void _comService_DataReceived(object sender, string e)
        {
            ReceivedData.AddRange(e.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
