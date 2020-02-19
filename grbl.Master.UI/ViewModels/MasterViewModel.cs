namespace grbl.Master.UI.ViewModels
{
    using Caliburn.Micro;
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Model.Properties;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using grbl.Master.UI.Converters;
    using grbl.Master.Utilities;
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
    using System.Threading;
    using System.Windows.Controls;

    using Xceed.Wpf.Toolkit;

    public class MasterViewModel : Screen
    {
        private readonly ICommandSender _commandSender;

        private readonly IComService _comService;

        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly IGrblDispatcher _grblDispatcher;

        private readonly IGrblStatus _grblStatus;

        private readonly Subject<Unit> _jogStopSubject = new Subject<Unit>();

        private readonly IMacroService _macroService;

        private int _joggingCount;

        private Macros _macrosSelected;

        private string _manualCommand;

        private double _selectedFeedRate = 1000;

        private double _selectedJoggingDistance = 10;

        public MasterViewModel(
            IComService comService,
            IGrblStatus grblStatus,
            ICommandSender commandSender,
            IGrblDispatcher grblDispatcher,
            IMacroService macroService)
        {
            _grblDispatcher = grblDispatcher;
            ComConnectionViewModel = new COMConnectionViewModel(comService);
            _comService = comService;
            _grblStatus = grblStatus;
            _commandSender = commandSender;
            _macroService = macroService;

            _macroService.LoadMacroses();

            _grblStatus.GrblStatusModel.MachineStateChanged += GrblStatusModelMachineStateChanged;
            _grblStatus.GrblStatusModel.PropertyChanged += GrblStatusModelPropertyChanged;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.CommunicationLogUpdated += CommandSenderCommunicationLogUpdated;
            _fileSystemWatcher.Deleted += FileSystemWatcherDeleted;
            _fileSystemWatcher.Created += FileSystemWatcherCreated;
            _fileSystemWatcher.Renamed += FileSystemWatcherRenamed;
            _fileSystemWatcher.Changed += FileSystemWatcherChanged;

            _commandSender.FileCommands.CommandList.CollectionChanged += (sender, args) =>
                {
                    NotifyOfPropertyChange(() => FileLinesProcessed);
                };

            StartNotifications();

            LoadAppSettings();
        }



        public string FilePath { get; set; }

        public FileState FileState { get; set; }

        private List<double> _joggingDistances =
            new List<double>
                {
                    0.01,
                    0.1,
                    1,
                    5,
                    10,
                    100
                };

        public List<double> JoggingDistances
        {
            get => _joggingDistances;
            set
            {
                _joggingDistances = value;
                SaveAppSettings();
                NotifyOfPropertyChange(nameof(JoggingDistances));
            }
        }

        private List<double> _feedRates =
            new List<double>
                {
                    5,
                    10,
                    50,
                    100,
                    500,
                    1000
                };

        public List<double> FeedRates
        {
            get => _feedRates;
            set
            {
                _feedRates = value;
                SaveAppSettings();
                NotifyOfPropertyChange(nameof(FeedRates));
            }
        }

        private void LoadAppSettings()
        {
            if (Settings.Default.JoggingDistances.Length > 0)
            {
                try
                {
                    _joggingDistances = Settings.Default.JoggingDistances.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList();
                    NotifyOfPropertyChange(nameof(JoggingDistances));
                }
                catch
                {
                    SaveAppSettings();
                }
            }

            if (Settings.Default.JoggingSpeeds.Length > 0)
            {
                try
                {
                    _feedRates = Settings.Default.JoggingSpeeds.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList();
                    NotifyOfPropertyChange(nameof(FeedRates));
                }
                catch
                {
                    SaveAppSettings();
                }
            }
        }

        private void SaveAppSettings()
        {
            var converter = new ListDoubleToString();
            Settings.Default.JoggingDistances = converter.Convert(
                JoggingDistances,
                typeof(string),
                null,
                CultureInfo.CurrentCulture) as string;

            Settings.Default.JoggingSpeeds = converter.Convert(
                                                 FeedRates,
                                                    typeof(string),
                                                    null,
                                                    CultureInfo.CurrentCulture) as string;
            Settings.Default.Save();
            Settings.Default.Reload();
        }

        public ObservableCollection<Macros> Macroses => _macroService.Macroses;

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

        public int FileLinesCount => _commandSender.FileCommands.CommandCount;

        public int FileLinesProcessed => FileCommandsCollection.Count;

        public GrblStatusModel GrblStatus => _grblStatus.GrblStatusModel;

        public ObservableCollection<string> CommunicationLog => _commandSender.CommunicationLog;

        public ObservableCollection<Command> ManualCommandsCollection => _commandSender.ManualCommands.CommandList;

        public ObservableCollection<Command> SystemCommandsCollection => _commandSender.SystemCommands.CommandList;

        public ObservableCollection<Command> FileCommandsCollection => _commandSender.FileCommands.CommandList;

        public string FileLines { get; set; } = "";

        public COMConnectionViewModel ComConnectionViewModel { get; }

        public List<string> Mask8Items { get; } = new List<string>
                                                      {
                                                          "0",
                                                          "1",
                                                          "2",
                                                          "3",
                                                          "4",
                                                          "5",
                                                          "6",
                                                          "7"
                                                      };

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

        public bool CanSetOffset => CanGCommand;

        public bool CanFileOpen => _commandSender.FileCommands.State == CommandSourceState.Stopped;

        public bool CanStartFileExecution => BasicCanSendCommand && FileLines.Any();

        public bool CanStartStepFileExecution =>
            BasicCanSendCommand && _commandSender.FileCommands.State != CommandSourceState.Running
                                     && FileLines.Any();

        public bool CanPauseFileExecution =>
            BasicCanSendCommand && _commandSender.FileCommands.State == CommandSourceState.Running
                                     && _commandSender.FileCommands.State != CommandSourceState.Paused;

        public bool CanStopFileExecution => _commandSender.FileCommands.State != CommandSourceState.Stopped;

        public bool CanReloadFile =>
            _commandSender.FileCommands.State == CommandSourceState.Stopped && File.Exists(FilePath);

        public bool CanSaveSettings => BasicCanSendCommand;

        public bool CanRunMacro => BasicCanSendCommand;

        private void FileSystemWatcherRenamed(object sender, RenamedEventArgs e)
        {
            if (e.OldName == Path.GetFileName(FilePath))
            {
                FileState = FileState.Deleted;
                NotifyOfPropertyChange(nameof(FileState));
            }
        }

        private void FileSystemWatcherCreated(object sender, FileSystemEventArgs e)
        {
            if (e.Name == Path.GetFileName(FilePath))
            {
                FileState = FileState.Updated;
                NotifyOfPropertyChange(nameof(FileState));
            }
        }

        private void FileSystemWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.Name == Path.GetFileName(FilePath))
            {
                FileState = FileState.Deleted;
                NotifyOfPropertyChange(nameof(FileState));
            }
        }

        private void FileSystemWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name == Path.GetFileName(FilePath))
            {
                FileState = FileState.Updated;
                NotifyOfPropertyChange(nameof(FileState));
            }
        }

        private void StartNotifications()
        {
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
        }

        public void SetToolLengthOffset(string val)
        {
            if (decimal.TryParse(
                val.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                out var value) && this._grblStatus.GrblStatusModel.ToolLengthOffset != value)
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
                FilePath = openFileDialog.FileName;
                _fileSystemWatcher.Path = Path.GetDirectoryName(FilePath);
                FileState = FileState.Unchanged;
                _fileSystemWatcher.EnableRaisingEvents = true;
                NotifyOfPropertyChange(nameof(FilePath));
                ReloadFile();
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
                _commandSender.FileCommands.Add(FileLines);
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
            if (File.Exists(FilePath))
            {
                FileLines = File.ReadAllText(FilePath);
                FileState = FileState.Unchanged;

                NotifyOfPropertyChange(() => FileLines);
                NotifyOfPropertyChange(nameof(FileState));
                NotifyCanCommands();
            }
        }

        public void ResetGrbl()
        {
            RealtimeIntCommand(24);
            _commandSender.PurgeQueues();
        }

        public void RunMacro(Macros macro)
        {
            var lines = macro.Command.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            _commandSender.ManualCommands.Add(lines);
        }

        public void DeleteMacro(Macros macro)
        {
            _macroService.DeleteMacros(macro);
        }

        public void EditMacro(Macros macro)
        {
            MacrosSelected = macro;
        }

        public void SaveMacro(Macros macro)
        {
            if (_macroService.Macroses.Any(x => x.Index == macro.Index) && macro.Index >= 0)
                _macroService.Macroses.RemoveAt(macro.Index);
            _macroService.Macroses.Insert(Math.Max(0, macro.Index), macro);
            _macroService.SaveMacroses();
            CancelMacro();
        }

        public void CancelMacro()
        {
            MacrosSelected = null;
        }

        public void AddMacro()
        {
            MacrosSelected = new Macros();
        }

        public void UpMacro(Macros macro)
        {
            if (_macroService.Macroses.Any(x => x.Index == macro.Index) && macro.Index > 0)
            {
                _macroService.Macroses.RemoveAt(macro.Index);
                _macroService.Macroses.Insert(Math.Max(0, macro.Index - 1), macro);
                _macroService.SaveMacroses();
            }
        }

        public void DownMacro(Macros macro)
        {
            if (_macroService.Macroses.Any(x => x.Index == macro.Index)
                && macro.Index < _macroService.Macroses.Max(x => x.Index))
            {
                _macroService.Macroses.RemoveAt(macro.Index);
                _macroService.Macroses.Insert(Math.Max(0, macro.Index + 1), macro);
                _macroService.SaveMacroses();
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