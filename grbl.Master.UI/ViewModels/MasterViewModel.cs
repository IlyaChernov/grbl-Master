using Caliburn.Micro;
using System;

namespace grbl.Master.UI.ViewModels
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using grbl.Master.UI.Properties;
    using System.Collections.ObjectModel;

    public class MasterViewModel : Screen
    {
        private readonly IComService _comService;
        private readonly IGrblStatusRequester _grblStatusRequester;
        private readonly ICommandSender _commandSender;
        private readonly IGrblStatusProcessor _grblStatusProcessor;
        private string _manualCommand;

        public MasterViewModel(IGrblStatusProcessor grblStatusProcessor, IComService comService, IGrblStatusRequester grblStatusRequester, ICommandSender commandSender)
        {
            ComConnectionViewModel = new COMConnectionViewModel(comService);
            _grblStatusProcessor = grblStatusProcessor;
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
            NotifyOfPropertyChange(() => CanUnlockCommand);
            NotifyOfPropertyChange(() => CanResetCommand);
            NotifyOfPropertyChange(() => CanHoldCommand);
            NotifyOfPropertyChange(() => CanStartCommand);
            NotifyOfPropertyChange(() => CanCheckCommand);
            NotifyOfPropertyChange(() => CanRegularSystemCommand);
        }

        public GrblStatus GrblStatus => _grblStatusProcessor.GrblStatus;

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

        public bool CanUnlockCommand => _comService.IsConnected;

        public void UnlockCommand()
        {
            _commandSender.Send("$X", CommandType.System);
        }

        public bool CanResetCommand => _comService.IsConnected;

        public void ResetCommand()
        {

            _commandSender.Send((char)24, CommandType.System); //Ctrl+X
        }

        public bool CanHoldCommand => _comService.IsConnected;

        public void HoldCommand()
        {
            _commandSender.Send("!", CommandType.System);
        }

        public bool CanStartCommand => _comService.IsConnected;

        public void StartCommand()
        {
            _commandSender.Send("~", CommandType.System);
        }

        public bool CanCheckCommand => _comService.IsConnected;

        public void CheckCommand()
        {
            _commandSender.Send("$C", CommandType.System);
        }

        public bool CanRegularSystemCommand => _comService.IsConnected;        

        public void RegularSystemCommand(string command)
        {
            _commandSender.Send(command, CommandType.System);
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            if (!_grblStatusRequester.IsRunning && e.Equals(Resources.GRBLWelcomeMessage))
            {
                _grblStatusRequester.StartRequesting(TimeSpan.FromMilliseconds(200));
            }
        }
    }
}
