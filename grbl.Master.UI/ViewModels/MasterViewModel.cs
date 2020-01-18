namespace grbl.Master.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;

    using Caliburn.Micro;

    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using grbl.Master.Utilities;

    public class MasterViewModel : Screen
    {
        private readonly ICommandSender _commandSender;

        private readonly IComService _comService;

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly IGrblDispatcher _grblDispatcher;

        private readonly IGrblStatus _grblStatus;

        private readonly Subject<Unit> _jogStopSubject = new Subject<Unit>();

        private string _manualCommand;

        private double _selectedJoggingDistance = 1;

        private double _selectedFeedRate = 500;

        public MasterViewModel(
            IComService comService,
            IGrblStatus grblStatus,
            ICommandSender commandSender,
            IGrblDispatcher grblDispatcher)
        {
            _grblDispatcher = grblDispatcher;
            ComConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _grblStatus = grblStatus;
            _commandSender = commandSender;

            _grblStatus.GrblStatusModel.MachineStateChanged += GrblStatusModelMachineStateChanged;
            _grblStatus.GrblStatusModel.PropertyChanged += GrblStatusModelPropertyChanged;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.CommandListUpdated += CommandSenderCommandListUpdated;
            _commandSender.CommunicationLogUpdated += CommandSenderCommunicationLogUpdated;
        }

        private void GrblStatusModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        public List<double> JoggingDistances => new List<double> { 0.01, 0.1, 1, 5, 10, 100 }; //todo: move to settings

        public List<double> FeedRates => new List<double> { 5, 10, 50, 100, 500, 800 }; //todo: move to settings

        public double SelectedJoggingDistance
        {
            get => _selectedJoggingDistance;
            set
            {
                _selectedJoggingDistance = value;
                NotifyOfPropertyChange(() => SelectedJoggingDistance);
            }
        }

        public double SelectedFeedRate
        {
            get => _selectedFeedRate;
            set
            {
                _selectedFeedRate = value;
                NotifyOfPropertyChange(() => SelectedFeedRate);
            }
        }

        public GrblStatusModel GrblStatus => _grblStatus.GrblStatusModel;

        public ObservableCollection<Command> CommandList => _commandSender.CommandList;

        public ObservableCollection<string> CommunicationLog => _commandSender.CommunicationLog;

        public COMConnectionViewModel ComConnectionViewModel { get; }

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

        public bool CanSendManualCommand =>
            !string.IsNullOrWhiteSpace(ManualCommand) && _comService.IsConnected;

        private bool BasicCanSendCommand =>
            _comService.IsConnected && _grblStatus.GrblStatusModel.MachineState != MachineState.Offline
                                         && _grblStatus.GrblStatusModel.MachineState != MachineState.Online;

        public bool CanSendEnterCommand => BasicCanSendCommand;

        public bool CanGCommand => _grblStatus.GrblStatusModel.MachineState != MachineState.Alarm && BasicCanSendCommand;

        public bool CanSystemCommand => BasicCanSendCommand;

        public bool CanSystemCommandNA => _grblStatus.GrblStatusModel.MachineState != MachineState.Alarm && BasicCanSendCommand;

        public bool CanRealtimeCommand => BasicCanSendCommand;

        public bool CanRealtimeCommandNA => _grblStatus.GrblStatusModel.MachineState != MachineState.Alarm && BasicCanSendCommand;

        public bool CanRealtimeIntCommand => BasicCanSendCommand;

        public bool CanJoggingCommand => _grblStatus.GrblStatusModel.MachineState != MachineState.Alarm && BasicCanSendCommand;

        public bool CanCancelJogging => BasicCanSendCommand;

        public bool CanSetToolLengthOffset => CanGCommand;

        public bool CanSetOffset => CanGCommand;

        private void NotifyCanCommands()
        {
            NotifyOfPropertyChange(() => CanSendManualCommand);
            NotifyOfPropertyChange(() => CanSendEnterCommand);
            NotifyOfPropertyChange(() => CanGCommand);
            NotifyOfPropertyChange(() => CanSystemCommand);
            NotifyOfPropertyChange(() => CanSystemCommandNA);
            NotifyOfPropertyChange(() => CanRealtimeCommand);
            NotifyOfPropertyChange(() => CanRealtimeCommandNA);
            NotifyOfPropertyChange(() => CanRealtimeIntCommand);
            NotifyOfPropertyChange(() => CanJoggingCommand);
            NotifyOfPropertyChange(() => CanSetToolLengthOffset);
            NotifyOfPropertyChange(() => CanSetOffset);
        }

        public void SetToolLengthOffset(string val)
        {
            GCommand($"G43.1 Z{val.ToGrblString()}", "$#");
        }

        public void SetOffset(string param, Control sender, string val)
        {
            GCommand($"{sender.Tag} {param}{val.ToGrblString()}", "$#");
        }

        private void GrblStatusModelMachineStateChanged(object sender, MachineState e)
        {
            NotifyCanCommands();
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
            NotifyCanCommands();
        }

        public void SendManualCommand()
        {
            _commandSender.SendAsync(ManualCommand);
        }

        public void SendEnterCommand()
        {
            SendManualCommand();
        }

        public void GCommand(object code)
        {
            GCommand(code.ToString());
        }

        public void GCommand(string code, string onResult = null)
        {
            var codeCoordinated = string.Format(
                code,
                _grblStatus.GrblStatusModel.WorkPosition.X.ToGrblString(),
                _grblStatus.GrblStatusModel.WorkPosition.Y.ToGrblString(),
                _grblStatus.GrblStatusModel.WorkPosition.Z.ToGrblString(),
                _grblStatus.GrblStatusModel.MachinePosition.X.ToGrblString(),
                _grblStatus.GrblStatusModel.MachinePosition.Y.ToGrblString(),
                _grblStatus.GrblStatusModel.MachinePosition.Z.ToGrblString()
                );
            _commandSender.SendAsync(codeCoordinated, onResult);
        }

        public void SystemCommand(string code)
        {
            _commandSender.SendAsync(code);
        }

        public void SystemCommandNA(string code)
        {
            SystemCommand(code);
        }

        public void RealtimeCommand(string code)
        {
            _commandSender.SendAsync(code);
        }

        public void RealtimeCommandNA(string code)
        {
            RealtimeCommand(code);
        }

        public void RealtimeIntCommand(int code)
        {
            _commandSender.SendAsync(new string(new[] { (char)code }));
        }

        private int _joggingCount;

        public void JoggingCommand(string code)
        {
            _joggingCount = 0;
            _jogStopSubject.OnNext(Unit.Default);
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).TakeUntil(_jogStopSubject).Subscribe(
                l =>
                    {
                        _joggingCount++;
                        _commandSender.SendAsync(
                            "$J=" + string.Format(code, SelectedJoggingDistance.ToGrblString(), SelectedFeedRate.ToGrblString()));
                    });
        }

        public void CancelJogging()
        {
            _jogStopSubject.OnNext(Unit.Default);
            if (_joggingCount > 1)
                RealtimeIntCommand(0x0085);
        }
    }
}