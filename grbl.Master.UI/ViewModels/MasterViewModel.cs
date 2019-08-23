using Caliburn.Micro;
using System;

namespace grbl.Master.UI.ViewModels
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System.Collections.ObjectModel;

    public class MasterViewModel : Screen
    {
        private readonly IGrblDispatcher _grblDispatcher;
        private readonly IComService _comService;
        private readonly IGrblStatus _grblStatus;
        private readonly ICommandSender _commandSender;


        private string _manualCommand;

        public MasterViewModel(IComService comService, IGrblStatus grblStatus, ICommandSender commandSender, IGrblDispatcher grblDispatcher)
        {
            _grblDispatcher = grblDispatcher;
            ComConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _grblStatus = grblStatus;
            _commandSender = commandSender;

            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.CommandListUpdated += CommandSenderCommandListUpdated;
            _commandSender.CommunicationLogUpdated += CommandSenderCommunicationLogUpdated;
        }

        private void CommandSenderCommunicationLogUpdated(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => CommunicationLog);
        }

        private void CommandSenderCommandListUpdated(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => CommandList);
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            NotifyOfPropertyChange(() => CanSendManualCommand);
            NotifyOfPropertyChange(() => CanSendEnterCommand);
            NotifyOfPropertyChange(() => CanUnlockCommand);
            NotifyOfPropertyChange(() => CanResetCommand);
            NotifyOfPropertyChange(() => CanHoldCommand);
            NotifyOfPropertyChange(() => CanStartCommand);
            NotifyOfPropertyChange(() => CanCheckCommand);
            NotifyOfPropertyChange(() => CanRegularSystemCommand);
        }

        public GrblStatusModel GrblStatus => _grblStatus.GrblStatusModel;

        public ObservableCollection<Command> CommandList => _commandSender.CommandList;

        public ObservableCollection<string> CommunicationLog => _commandSender.CommunicationLog;

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
            _commandSender.Send(ManualCommand, CommandType.GCode);
        }

        public bool CanSendEnterCommand => _comService.IsConnected;

        public void SendEnterCommand()
        {
            SendManualCommand();
        }

        public bool CanUnlockCommand => _comService.IsConnected;

        public void UnlockCommand()
        {
            _commandSender.SendSystem("$X");
        }

        public bool CanResetCommand => _comService.IsConnected;

        public void ResetCommand()
        {
            _commandSender.SendRealtime((char)24); //Ctrl+X
        }

        public bool CanHoldCommand => _comService.IsConnected;

        public void HoldCommand()
        {
            _commandSender.SendRealtime('!');
        }

        public bool CanStartCommand => _comService.IsConnected;

        public void StartCommand()
        {
            _commandSender.SendRealtime('~');
        }

        public bool CanCheckCommand => _comService.IsConnected;

        public void CheckCommand()
        {
            _commandSender.SendSystem("$C");
        }

        public bool CanRegularSystemCommand => _comService.IsConnected;

        public void RegularSystemCommand(string command)
        {
            _commandSender.Send(command, CommandType.System);
        }
    }
}
