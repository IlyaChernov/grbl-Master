namespace grbl.Master.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    using Caliburn.Micro;

    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;

    public class MasterViewModel : Screen
    {
        private readonly ICommandSender _commandSender;

        private readonly IComService _comService;

        private readonly IGrblDispatcher _grblDispatcher;

        private readonly IGrblStatus _grblStatus;

        private readonly Subject<Unit> _jogStopSubject = new Subject<Unit>();

        private string _manualCommand;

        private double _manualDistance = 1;

        private double _manualSpeed = 500;

        public MasterViewModel(
            IComService comService,
            IGrblStatus grblStatus,
            ICommandSender commandSender,
            IGrblDispatcher grblDispatcher)
        {
            this._grblDispatcher = grblDispatcher;
            this.ComConnectionViewModel = new COMConnectionViewModel(comService);
            this._comService = comService;
            this._grblStatus = grblStatus;
            this._commandSender = commandSender;

            this._grblStatus.GrblStatusModel.MachineStateChanged += this.GrblStatusModelMachineStateChanged;
            this._comService.ConnectionStateChanged += this.ComServiceConnectionStateChanged;
            this._commandSender.CommandListUpdated += this.CommandSenderCommandListUpdated;
            this._commandSender.CommunicationLogUpdated += this.CommandSenderCommunicationLogUpdated;
        }

        public double ManualDistance
        {
            get => this._manualDistance;
            set
            {
                this._manualDistance = value;
                this.NotifyOfPropertyChange(() => this.ManualDistance);
            }
        }

        public double ManualSpeed
        {
            get => this._manualSpeed;
            set
            {
                this._manualSpeed = value;
                this.NotifyOfPropertyChange(() => this.ManualSpeed);
            }
        }

        public GrblStatusModel GrblStatus => this._grblStatus.GrblStatusModel;

        public ObservableCollection<Command> CommandList => this._commandSender.CommandList;

        public ObservableCollection<string> CommunicationLog => this._commandSender.CommunicationLog;

        public COMConnectionViewModel ComConnectionViewModel { get; }

        public string ManualCommand
        {
            get => this._manualCommand;
            set
            {
                this._manualCommand = value;
                this.NotifyOfPropertyChange(() => this.ManualCommand);
                this.NotifyOfPropertyChange(() => this.CanSendManualCommand);
            }
        }

        public bool CanSendManualCommand =>
            !string.IsNullOrWhiteSpace(this.ManualCommand) && this._comService.IsConnected;

        private bool BasicCanSendCommand =>
            this._comService.IsConnected && this._grblStatus.GrblStatusModel.MachineState != MachineState.Offline
                                         && this._grblStatus.GrblStatusModel.MachineState != MachineState.Online;

        public bool CanSendEnterCommand => this.BasicCanSendCommand;

        public bool CanGCommand => this.BasicCanSendCommand;

        public bool CanSystemCommand => this.BasicCanSendCommand;

        public bool CanRealtimeCommand => this.BasicCanSendCommand;

        public bool CanRealtimeIntCommand => this.BasicCanSendCommand;

        public bool CanJoggingCommand => this.BasicCanSendCommand;

        public bool CanCancelJogging => this.BasicCanSendCommand;

        private void GrblStatusModelMachineStateChanged(object sender, MachineState e)
        {
            this.NotifyOfPropertyChange(() => this.CanSendManualCommand);
            this.NotifyOfPropertyChange(() => this.CanSendEnterCommand);
            this.NotifyOfPropertyChange(() => this.CanGCommand);
            this.NotifyOfPropertyChange(() => this.CanSystemCommand);
            this.NotifyOfPropertyChange(() => this.CanRealtimeCommand);
            this.NotifyOfPropertyChange(() => this.CanRealtimeIntCommand);
            this.NotifyOfPropertyChange(() => this.CanJoggingCommand);
        }

        private void CommandSenderCommunicationLogUpdated(object sender, EventArgs e)
        {
            this.NotifyOfPropertyChange(() => this.CommunicationLog);
        }

        private void CommandSenderCommandListUpdated(object sender, EventArgs e)
        {
            this.NotifyOfPropertyChange(() => this.CommandList);
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            this.NotifyOfPropertyChange(() => this.CanSendManualCommand);
            this.NotifyOfPropertyChange(() => this.CanSendEnterCommand);
            this.NotifyOfPropertyChange(() => this.CanGCommand);
            this.NotifyOfPropertyChange(() => this.CanSystemCommand);
            this.NotifyOfPropertyChange(() => this.CanRealtimeCommand);
            this.NotifyOfPropertyChange(() => this.CanRealtimeIntCommand);
            this.NotifyOfPropertyChange(() => this.CanJoggingCommand);
        }

        public void SendManualCommand()
        {
            this._commandSender.SendGCode(this.ManualCommand);
        }

        public void SendEnterCommand()
        {
            this.SendManualCommand();
        }

        public void GCommand(string code)
        {
            this._commandSender.SendGCode(code);
        }

        public void SystemCommand(string code)
        {
            this._commandSender.SendSystem(code);
        }

        public void RealtimeCommand(string code)
        {
            this._commandSender.SendRealtime(code);
        }

        public void RealtimeIntCommand(int code)
        {
            this._commandSender.SendRealtime(((char)code).ToString());
        }

        public void JoggingCommand(string code)
        {
            this._jogStopSubject.OnNext(Unit.Default);
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1)).TakeUntil(this._jogStopSubject).Subscribe(
                l =>
                    {
                        this._commandSender.SendGCode(
                            "$J=" + string.Format(code, this.ManualDistance, this.ManualSpeed));
                    });
        }

        public void CancelJogging()
        {
            this._jogStopSubject.OnNext(Unit.Default);
            if (this._grblStatus.GrblStatusModel.MachineState == MachineState.Jog) this.RealtimeCommand("!");
        }
    }
}