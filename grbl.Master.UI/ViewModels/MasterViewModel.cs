using Caliburn.Micro;
using System;

namespace grbl.Master.UI.ViewModels
{
    using grbl.Master.Model;
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System.Collections.ObjectModel;

    using grbl.Master.BL.Interface;

    public class MasterViewModel : Screen
    {
        private readonly IComService _comService;
        private readonly IGrblStatusRequester _grblStatusRequester;
        private readonly ICommandSender _commandSender;
        private string _manualCommand;

        private GrblStatus _grblStatus;

        public MasterViewModel(IComService comService, IGrblStatusRequester grblStatusRequester, ICommandSender commandSender)
        {
            ComConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _grblStatusRequester = grblStatusRequester;
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
                _grblStatusRequester.StopRequesting();
            }

            NotifyOfPropertyChange(() => CanSendManualCommand);
            NotifyOfPropertyChange(() => CanSendEnterCommand);
        }

        public ObservableCollection<Command> CommandList => _commandSender.CommandList;

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

        public GrblStatus GrblStatus
        {
            get => _grblStatus;
            set
            {
                _grblStatus = value;
                NotifyOfPropertyChange(() => GrblStatus);
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

        private void ComServiceLineReceived(object sender, string e)
        {
            if (!_grblStatusRequester.IsRunning)
            {
                _grblStatusRequester.StartRequesting(TimeSpan.FromMilliseconds(200));
            }
        }
    }
}
