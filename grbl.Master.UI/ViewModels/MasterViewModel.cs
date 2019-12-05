﻿namespace grbl.Master.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
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
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.CommandListUpdated += CommandSenderCommandListUpdated;
            _commandSender.CommunicationLogUpdated += CommandSenderCommunicationLogUpdated;
        }

        public List<double> JoggingDistances => new List<double> { 0.001, 0.01, 0.1, 1, 5, 10, 100 };

        public List<double> FeedRates => new List<double> {5, 10, 50, 100, 500, 800 };


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

        public bool CanGCommand => BasicCanSendCommand;

        public bool CanSystemCommand => BasicCanSendCommand;

        public bool CanRealtimeCommand => BasicCanSendCommand;

        public bool CanRealtimeIntCommand => BasicCanSendCommand;

        public bool CanJoggingCommand => BasicCanSendCommand;

        public bool CanCancelJogging => BasicCanSendCommand;

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

        public void SendManualCommand()
        {
            _commandSender.SendAsync(ManualCommand);
        }

        public void SendEnterCommand()
        {
            SendManualCommand();
        }

        public void GCommand(string code)
        {
            _commandSender.SendAsync(code);
        }

        public void SystemCommand(string code)
        {
            _commandSender.SendAsync(code);
        }

        public void RealtimeCommand(string code)
        {
            _commandSender.SendAsync(code);
        }

        public void RealtimeIntCommand(int code)
        {
            _commandSender.SendAsync(new string(new[] { (char)code }));
        }

        private int _joggingCount;

        public void JoggingCommand(string code)
        {
            this._joggingCount = 0;
            _jogStopSubject.OnNext(Unit.Default);
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).TakeUntil(_jogStopSubject).Subscribe(
                l =>
                    {
                        this._joggingCount++;
                        _commandSender.SendAsync(
                            "$J=" + string.Format(code, SelectedJoggingDistance, SelectedFeedRate));
                    });
        }

        public void CancelJogging()
        {
            _jogStopSubject.OnNext(Unit.Default);
            if (this._joggingCount > 1)
                RealtimeIntCommand(0x0085);
        }
    }
}