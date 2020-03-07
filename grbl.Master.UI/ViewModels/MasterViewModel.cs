namespace grbl.Master.UI.ViewModels
{
    using Caliburn.Micro;
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using grbl.Master.Utilities;
    using ICSharpCode.AvalonEdit.Highlighting;
    using ICSharpCode.AvalonEdit.Highlighting.Xshd;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Controls;
    using System.Xml;
    using Xceed.Wpf.Toolkit;

    public class MasterViewModel : Screen
    {
        private readonly ICommandSender _commandSender;

        private readonly IComService _comService;

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly IGrblDispatcher _grblDispatcher;

        private readonly IGrblStatus _grblStatus;

        private readonly Subject<Unit> _jogStopSubject = new Subject<Unit>();

        private readonly IApplicationSettingsService _applicationSettingsService;

        private readonly IGCodeFileService _gCodeFileService;

        private int _joggingCount;

        private Macros _macrosSelected;

        private string _manualCommand;

        //private double _selectedFeedRate = 1000;

        //private double _selectedJoggingDistance = 10;

        public MasterViewModel(
            IComService comService,
            IGrblStatus grblStatus,
            ICommandSender commandSender,
            IGrblDispatcher grblDispatcher,
            IApplicationSettingsService applicationSettingsService,
            IGCodeFileService gCodeFileService)
        {
            _grblDispatcher = grblDispatcher;
            _applicationSettingsService = applicationSettingsService;
            _applicationSettingsService.Upgrade();
            _applicationSettingsService.Load();

            ComConnectionViewModel = new COMConnectionViewModel(comService, applicationSettingsService);
            _comService = comService;
            _grblStatus = grblStatus;
            _commandSender = commandSender;
            _gCodeFileService = gCodeFileService;

            _grblStatus.GrblStatusModel.MachineStateChanged += GrblStatusModelMachineStateChanged;
            _grblStatus.GrblStatusModel.PropertyChanged += GrblStatusModelPropertyChanged;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.CommunicationLogUpdated += CommandSenderCommunicationLogUpdated;

            _commandSender.FileCommands.CommandList.CollectionChanged += (sender, args) =>
                {
                    NotifyOfPropertyChange(() => FileLinesProcessed);
                };

            StartNotifications();
        }

        private IHighlightingDefinition _GCodeHighlighting;

        public IHighlightingDefinition GCodeHighlighting
        {
            get
            {
                return _GCodeHighlighting ??= GetHighlightingDefinition();
            }
        }

        private IHighlightingDefinition GetHighlightingDefinition()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using Stream s = assembly.GetManifestResourceStream("grbl.Master.UI.GCodeHighlighting.xshd");
            using XmlTextReader reader = new XmlTextReader(s);
            return HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        public ObservableCollection<double> JoggingDistances
        {
            get => _applicationSettingsService.Settings.JoggingDistances;
            set
            {
                _applicationSettingsService.Settings.JoggingDistances = value;
                _applicationSettingsService.Save();
            }
        }

        public ObservableCollection<double> FeedRates
        {
            get => _applicationSettingsService.Settings.FeedRates;
            set
            {
                _applicationSettingsService.Settings.FeedRates = value;
                _applicationSettingsService.Save();
            }
        }

        public ObservableCollection<Macros> Macroses => _applicationSettingsService.Settings.Macroses;

        public TimeSpan Elapsed => _commandSender.FileCommands.Elapsed;

        public Macros MacrosSelected
        {
            get => _macrosSelected;
            set
            {
                _macrosSelected = value;
                NotifyOfPropertyChange(() => MacrosSelected);
                NotifyOfPropertyChange(() => IsMacrosSelected);
            }
        }

        public bool IsMacrosSelected => MacrosSelected != null;

        public double SelectedJoggingDistance
        {
            get => _applicationSettingsService.Settings.JoggingDistance;
            set
            {
                _applicationSettingsService.Settings.JoggingDistance = value;
                _applicationSettingsService.Save();
                NotifyOfPropertyChange(() => SelectedJoggingDistance);
            }
        }

        public double SelectedFeedRate
        {
            get => _applicationSettingsService.Settings.FeedRate;
            set
            {
                _applicationSettingsService.Settings.FeedRate = value;
                _applicationSettingsService.Save();
                NotifyOfPropertyChange(() => SelectedFeedRate);
            }
        }

        public int FileLinesCount => _commandSender.FileCommands.CommandCount;

        public int FileLinesProcessed => FileCommandsCollection.Count;

        public GrblStatusModel GrblStatus => _grblStatus.GrblStatusModel;

        public ObservableCollection<string> CommunicationLog => _commandSender.CommunicationLog;

        public ObservableCollection<Command> ManualCommandsCollection => _commandSender.ManualCommands.CommandList;

        public ObservableCollection<Command> SystemCommandsCollection => _commandSender.SystemCommands.CommandList;

        public ObservableCollection<Command> FileCommandsCollection => _commandSender.FileCommands.CommandList;

        public GCodeFile GCodeFile => _gCodeFileService.File;

        public COMConnectionViewModel ComConnectionViewModel { get; }

        public List<string> Mask8Items { get; } = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7" };

        public List<string> Mask3Items { get; } = new List<string> { "0", "1", "2" };

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

        public bool CanGCommand =>
            _grblStatus.GrblStatusModel.MachineState != MachineState.Alarm && BasicCanSendCommand
                                                                                && _commandSender.FileCommands
                                                                                    .State != CommandSourceState
                                                                                    .Running;

        public bool CanSystemCommand => BasicCanSendCommand;

        public bool CanSystemCommandNA =>
            _grblStatus.GrblStatusModel.MachineState != MachineState.Alarm && BasicCanSendCommand;

        public bool CanRealtimeCommand => BasicCanSendCommand;

        public bool CanRealtimeCommandNA =>
            _grblStatus.GrblStatusModel.MachineState != MachineState.Alarm && BasicCanSendCommand;

        public bool CanRealtimeIntCommand => BasicCanSendCommand;

        public bool CanResetGrbl => BasicCanSendCommand;

        public bool CanJoggingCommand =>
            _grblStatus.GrblStatusModel.MachineState != MachineState.Alarm && BasicCanSendCommand;

        public bool CanCancelJogging => BasicCanSendCommand;

        public bool CanSetToolLengthOffset => CanGCommand;

        public bool CanSetWorkPos => CanGCommand && _grblStatus.GrblStatusModel.MachineState == MachineState.Idle;

        public bool CanAccessoryState => _grblStatus.GrblStatusModel.MachineState == MachineState.Hold;

        public bool CanSetOffset => CanGCommand;

        public bool CanFileOpen => _commandSender.FileCommands.State == CommandSourceState.Stopped;

        public bool CanStartFileExecution => BasicCanSendCommand && _gCodeFileService.File.FileData.Text.Any();

        public bool CanStartStepFileExecution =>
            BasicCanSendCommand && _commandSender.FileCommands.State != CommandSourceState.Running
                                     && _gCodeFileService.File.FileData.Text.Any();

        public bool CanPauseFileExecution =>
            BasicCanSendCommand && _commandSender.FileCommands.State == CommandSourceState.Running
                                     && _commandSender.FileCommands.State != CommandSourceState.Paused;

        public bool CanStopFileExecution => _commandSender.FileCommands.State != CommandSourceState.Stopped;

        public bool CanReloadFile =>
            _commandSender.FileCommands.State == CommandSourceState.Stopped && File.Exists(_gCodeFileService.File.FilePath);

        public bool CanSaveSettings => BasicCanSendCommand;

        public bool CanRunMacro => BasicCanSendCommand;

        private void StartNotifications()
        {
            NotifyOfPropertyChange(nameof(ApplicationSettings));

            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1)). /*TakeUntil(_stopSubject).*/
                Subscribe(l => { NotifyOfPropertyChange(nameof(Elapsed)); });
        }

        private void GrblStatusModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
            NotifyCanCommands();
        }

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
            NotifyOfPropertyChange(() => CanResetGrbl);
            NotifyOfPropertyChange(() => CanFileOpen);
            NotifyOfPropertyChange(() => CanStartFileExecution);
            NotifyOfPropertyChange(() => CanStartStepFileExecution);
            NotifyOfPropertyChange(() => CanPauseFileExecution);
            NotifyOfPropertyChange(() => CanStopFileExecution);
            NotifyOfPropertyChange(() => CanReloadFile);
            NotifyOfPropertyChange(() => CanSaveSettings);
            NotifyOfPropertyChange(() => CanRunMacro);
            NotifyOfPropertyChange(nameof(CanSetWorkPos));
            NotifyOfPropertyChange(nameof(CanAccessoryState));
        }

        public void SetToolLengthOffset(string val)
        {
            if (decimal.TryParse(
                val.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                out var value) && _grblStatus.GrblStatusModel.ToolLengthOffset != value)
            {
                GCommand($"G43.1 Z{val.ToGrblString()}", "$#");
            }
        }

        public void SetWorkPos(string param, Control sender)
        {
            var expectedValue = ((CalculatorUpDown)sender).Value;

            GCommand($"G90 G10 L20 P0 {param}{expectedValue.ToString().ToGrblString()}");
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
                _grblStatus.GrblStatusModel.MachinePosition.Z.ToGrblString());
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

        public void JoggingCommand(string code)
        {
            _joggingCount = 0;
            _jogStopSubject.OnNext(Unit.Default);

            var requestspeed = (SelectedJoggingDistance / (SelectedFeedRate / 60000)) * 0.9;

            _commandSender.SendAsync(
                "$J=" + string.Format(
                    code,
                    SelectedJoggingDistance.ToGrblString(),
                    SelectedFeedRate.ToGrblString()));

            Observable.Timer(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(requestspeed)).TakeUntil(_jogStopSubject).Subscribe(
                l =>
                    {
                        _joggingCount++;
                        _commandSender.SendAsync(
                            "$J=" + string.Format(
                                code,
                                SelectedJoggingDistance.ToGrblString(),
                                SelectedFeedRate.ToGrblString()));
                    });
        }

        public void CancelJogging()
        {
            _jogStopSubject.OnNext(Unit.Default);
            if (_joggingCount >= 1)
                RealtimeIntCommand(0x0085);
        }

        public void FileOpen()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter =
                                             "G-Code files (*.nc;*.g;*.gcode)|*.nc;*.g;*.gcode|All text files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true && File.Exists(openFileDialog.FileName))
            {
                _gCodeFileService.Load(openFileDialog.FileName);
                NotifyOfPropertyChange(nameof(GCodeFile));
                NotifyCanCommands();
            }
        }

        public void StartStepFileExecution()
        {
            StartFile(CommandSourceRunMode.LineByLine);
        }

        public void StartFileExecution()
        {
            StartFile(CommandSourceRunMode.StopInTheEnd);
        }

        private void StartFile(CommandSourceRunMode mode)
        {
            if (_commandSender.FileCommands.State == CommandSourceState.Stopped)
            {
                _commandSender.FileCommands.Purge();
                _commandSender.FileCommands.Add(_gCodeFileService.File.FileData.Text);
                NotifyOfPropertyChange(() => FileLinesCount);
            }

            _commandSender.FileCommands.Mode = mode;
            _commandSender.FileCommands.StartProcessing();
            NotifyCanCommands();
        }

        public void PauseFileExecution()
        {
            _commandSender.FileCommands.PauseProcessing();
            NotifyCanCommands();
        }

        public void StopFileExecution()
        {
            _commandSender.FileCommands.StopProcessing();
            NotifyCanCommands();
        }

        public void ReloadFile()
        {
            _gCodeFileService.Load(_gCodeFileService.File.FilePath);
            NotifyOfPropertyChange(nameof(GCodeFile));
            NotifyCanCommands();
        }

        public void ResetGrbl()
        {
            RealtimeIntCommand(24);
            _commandSender.PurgeQueues();
        }

        public void RunMacro(Macros macro)
        {
            var lines = macro.Command.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            _commandSender.ManualCommands.Add(lines);
        }

        public void DeleteMacro(Macros macro)
        {
            _applicationSettingsService.Settings.Macroses.Remove(macro);
            _applicationSettingsService.Save();
        }

        public void EditMacro(Macros macro)
        {
            MacrosSelected = macro;
        }

        public void SaveMacro(Macros macro)
        {
            if (_applicationSettingsService.Settings.Macroses.Any(x => x.Index == macro.Index) && macro.Index >= 0)
                _applicationSettingsService.Settings.Macroses.RemoveAt(macro.Index);
            _applicationSettingsService.Settings.Macroses.Insert(Math.Max(0, macro.Index), macro);
            _applicationSettingsService.Save();
            CancelMacro();
        }

        public void CancelMacro()
        {
            MacrosSelected = null;
        }

        public void AddMacro()
        {
            MacrosSelected = new Macros{Command = ""};
        }

        public void UpMacro(Macros macro)
        {
            if (_applicationSettingsService.Settings.Macroses.Any(x => x.Index == macro.Index) && macro.Index > 0)
            {
                _applicationSettingsService.Settings.Macroses.RemoveAt(macro.Index);
                _applicationSettingsService.Settings.Macroses.Insert(Math.Max(0, macro.Index - 1), macro);
                _applicationSettingsService.Save();
            }
        }

        public void DownMacro(Macros macro)
        {
            if (_applicationSettingsService.Settings.Macroses.Any(x => x.Index == macro.Index)
                && macro.Index < _applicationSettingsService.Settings.Macroses.Max(x => x.Index))
            {
                _applicationSettingsService.Settings.Macroses.RemoveAt(macro.Index);
                _applicationSettingsService.Settings.Macroses.Insert(Math.Max(0, macro.Index + 1), macro);
                _applicationSettingsService.Save();
            }
        }

        public void SaveSettings()
        {
            foreach (var grblSetting in _grblStatus.GrblStatusModel.Settings.SettingsList)
                if (grblSetting.Value != grblSetting.OriginalValue)
                    SystemCommand($"${grblSetting.Index} = {grblSetting.Value}");
            Observable.Start(() => Thread.Sleep(200)).Subscribe(unit => { SystemCommand("$$"); });
        }
    }
}