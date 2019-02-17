using Caliburn.Micro;
using System;

namespace grbl.Master.UI.ViewModels
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Interface;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using grbl.Master.Service.Enum;

    public class MasterViewModel : Screen
    {
        private readonly IComService _comService;
        private readonly IGrblStatusRequester _grblStatus;
        private readonly ICommandSender _commandSender;        
        private string _manualCommand;

        public MasterViewModel(IComService comService, IGrblStatusRequester grblStatus, ICommandSender commandSender)
        {
            ComConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _grblStatus = grblStatus;
            _commandSender = commandSender;

            _comService.LineReceived += ComServiceLineReceived;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.CommandListUpdated += CommandSenderCommandListUpdated;
        }

        private void CommandSenderCommandListUpdated(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => CommandList);
        }

        private void ComServiceConnectionStateChanged(object sender, EventArgs e)
        {
            if (!_comService.IsConnected)
            {
                _grblStatus.StopRequesting();
            }

            NotifyOfPropertyChange(() => CanSendManualCommand);
            NotifyOfPropertyChange(() => CanSendEnterCommand);
        }

        //public List<Command> CommandList => _commandSender.CommandList;//.ToList();
        public ObservableCollection<Command> CommandList => _commandSender.CommandList;//.ToList();


        public COMConnectionViewModel ComConnectionViewModel
        {
            get;
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
            _commandSender.Send(ManualCommand, CommandType.System);            
        }

        public bool CanSendEnterCommand => _comService.IsConnected;

        public void SendEnterCommand()
        {
            SendManualCommand();
        }

        private void ComServiceDataReceived(object sender, string e)
        {            
            if (!_grblStatus.IsRunning)
            {
                _grblStatus.StartRequesting(TimeSpan.FromMilliseconds(200));
            }
        }

        private void ComServiceLineReceived(object sender, string e)
        {            
            if (!_grblStatus.IsRunning)
            {
                _grblStatus.StartRequesting(TimeSpan.FromMilliseconds(200));
            }
        }
    }
}
