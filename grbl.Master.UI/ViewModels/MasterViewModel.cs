using Caliburn.Micro;
using System;

namespace grbl.Master.UI.ViewModels
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
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

        private double _manualDistance = 1;
        public double ManualDistance
        {
            get => _manualDistance;
            set
            {
                _manualDistance = value;
                NotifyOfPropertyChange(() => ManualDistance);
            }
        }
        private double _manualSpeed = 500;

        public double ManualSpeed
        {
            get => _manualSpeed;
            set
            {
                _manualSpeed = value;
                NotifyOfPropertyChange(() => ManualSpeed);
            }
        }


        private string _manualCommand;

        public MasterViewModel(IComService comService, IGrblStatus grblStatus, ICommandSender commandSender, IGrblDispatcher grblDispatcher)
        {
            _grblDispatcher = grblDispatcher;
            ComConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _grblStatus = grblStatus;
            _commandSender = commandSender;

            _grblStatus.GrblStatusModel.MachineStateChanged += GrblStatusModelMachineStateChanged;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.CommandListUpdated += CommandSenderCommandListUpdated;
            _commandSender.CommunicationLogUpdated += CommandSenderCommunicationLogUpdated;
        }

        private void GrblStatusModelMachineStateChanged(object sender, MachineState e)
        {
            NotifyOfPropertyChange(() => CanSendManualCommand);
            NotifyOfPropertyChange(() => CanSendEnterCommand);
            NotifyOfPropertyChange(() => CanGCommand);
            NotifyOfPropertyChange(() => CanSystemCommand);
            NotifyOfPropertyChange(() => CanRealtimeCommand);
            NotifyOfPropertyChange(() => CanRealtimeIntCommand);
            NotifyOfPropertyChange(() => CanJoggingCommand);
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
            NotifyOfPropertyChange(() => CanGCommand);
            NotifyOfPropertyChange(() => CanSystemCommand);
            NotifyOfPropertyChange(() => CanRealtimeCommand);
            NotifyOfPropertyChange(() => CanRealtimeIntCommand);
            NotifyOfPropertyChange(() => CanJoggingCommand);
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
            _commandSender.SendGCode(ManualCommand);
        }

        private bool BasicCanSendCommand =>
            _comService.IsConnected && _grblStatus.GrblStatusModel.MachineState != MachineState.Offline
                                    && _grblStatus.GrblStatusModel.MachineState != MachineState.Online;

        public bool CanSendEnterCommand => BasicCanSendCommand;

        public void SendEnterCommand()
        {
            SendManualCommand();
        }

        public bool CanGCommand => BasicCanSendCommand;

        public void GCommand(string code)
        {
            _commandSender.SendGCode(code);
        }

        public bool CanSystemCommand => BasicCanSendCommand;

        public void SystemCommand(string code)
        {
            _commandSender.SendSystem(code);
        }

        public bool CanRealtimeCommand => BasicCanSendCommand;

        public void RealtimeCommand(string code)
        {
            _commandSender.SendRealtime(code);
        }

        public bool CanRealtimeIntCommand => BasicCanSendCommand;

        public void RealtimeIntCommand(int code)
        {
            _commandSender.SendRealtime(((char)code).ToString());
        }

        public bool CanJoggingCommand => BasicCanSendCommand;

        public void JoggingCommand(string code)
        {
            _commandSender.SendGCode("$J=" + string.Format(code, ManualDistance, ManualSpeed));
        }
    }
}
