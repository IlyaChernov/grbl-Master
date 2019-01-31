using Caliburn.Micro;
using grbl.Master.Communication;
using System;

namespace grbl.Master.UI.ViewModels
{
    public class MasterViewModel : Screen
    {
        private ICOMService _comService;
        private BindableCollection<string> _receivedData = new BindableCollection<string>();

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

        private void _comService_DataReceived(object sender, string e)
        {
            ReceivedData.AddRange(e.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
