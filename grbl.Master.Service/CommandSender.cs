namespace grbl.Master.Service
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;

    using grbl.Master.Common.Enum;
    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Utilities;

    public class CommandSender : ICommandSender
    {
        private readonly IComService _comService;

        private readonly IGrblResponseTypeFinder _responseTypeFinder;

        private readonly IGrblCommandPreProcessor _commandPreProcessor;

        private readonly object _lockObject = new object();

        private bool _processing;

        private readonly SynchronizationContext _uiContext;

        private readonly Subject<Unit> _stopSubject = new Subject<Unit>();

        private readonly ConcurrentQueue<Command> _waitingCommandQueue = new ConcurrentQueue<Command>();

        private int _bufferSizeLimit = 100;

        public CommandSender(IComService comService, IGrblResponseTypeFinder responseTypeFinder, IGrblCommandPreProcessor commandPreProcessor)
        {
            _uiContext = SynchronizationContext.Current;
            _comService = comService;
            _responseTypeFinder = responseTypeFinder;
            _commandPreProcessor = commandPreProcessor;
            _comService.LineReceived += ComServiceLineReceived;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
        }

        private void ProcessQueue()
        {
            _processing = true;
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(10)).TakeUntil(_stopSubject).Subscribe(
                l =>
                    {
                        if ((SystemCommands.TryPeekCommand(out var cmd) || ManualCommands.TryPeekCommand(out cmd) || FileCommands.TryPeekCommand(out cmd) || FileCommands.State != CommandSourceState.Running && _waitingCommandQueue.Count == 0 && MacroCommands.TryPeekCommand(out cmd)) && cmd != null && cmd.Data.RemoveSpace().Length + CommandQueueLength <= _bufferSizeLimit)
                        {
                            var continueProcess = cmd.Source switch
                            {
                                CommandSourceType.System => SystemCommands.TryGetCommand(out cmd),
                                CommandSourceType.Macros => MacroCommands.TryGetCommand(out cmd),
                                CommandSourceType.Manual => ManualCommands.TryGetCommand(out cmd),
                                CommandSourceType.File => FileCommands.TryGetCommand(out cmd),
                                _ => false
                            };

                            if (!continueProcess) return;

                            _commandPreProcessor.Process(ref cmd);

                            Send(cmd);
                        }
                    });
        }

        public event EventHandler<int> CommandQueueLengthChanged;

        public CommandSource SystemCommands { get; } = new CommandSource(CommandSourceType.System, CommandSourceRunMode.Infinite);

        public CommandSource MacroCommands { get; } = new CommandSource(CommandSourceType.Macros, CommandSourceRunMode.LineByOk);

        public CommandSource ManualCommands { get; } = new CommandSource(CommandSourceType.Manual, CommandSourceRunMode.Infinite);

        public CommandSource FileCommands { get; } = new CommandSource(CommandSourceType.File, CommandSourceRunMode.StopInTheEnd);

        public int CommandQueueLength => _waitingCommandQueue.Sum(x => x.Data.RemoveSpace().Length);

        public ConcurrentQueue<Command> WaitingCommandQueue => _waitingCommandQueue;

        public ObservableCollection<string> CommunicationLog { get; } = new ObservableCollection<string>();

        public event EventHandler<Response> ResponseReceived;

        public event EventHandler CommandListUpdated;

        public event EventHandler CommunicationLogUpdated;

        public event EventHandler<Command> CommandFinished;

        public void Send(string command, string onResult = null)
        {
            lock (_lockObject)
            {
                _uiContext.Send(
                    x =>
                        {
                            CommunicationLog.Insert(0, "grbl <=" + command);
                            if (CommunicationLog.Count > 500)
                            {
                                CommunicationLog.Remove(CommunicationLog.Last());
                            }
                        }, null);
            }

            OnCommunicationLogUpdated();

            var cmd = new Command { Data = command, CommandOnResult = onResult };

            _commandPreProcessor.Process(ref cmd);

            switch (cmd.Type)
            {
                case RequestType.Realtime:
                    Send(cmd);
                    break;
                case RequestType.System:
                    {
                        lock (_lockObject)
                        {
                            if (!SystemCommands.DataInQueue(cmd.Data, 3))
                            {
                                SystemCommands.Add(command);
                            }
                        }

                        break;
                    }
                default:
                    {
                        lock (_lockObject)
                        {
                            ManualCommands.Add(command);
                        }

                        break;
                    }
            }
        }

        public void SendAsync(string command, string onResult = null)
        {
            Observable.Start(() => { Send(command, onResult); }).Subscribe();
        }

        public void PurgeQueues()
        {
            while (_waitingCommandQueue.Count > 0)
            {
                _waitingCommandQueue.TryDequeue(out var dummy);
                CommandQueueLengthChanged?.Invoke(this, CommandQueueLength);
            }

            SystemCommands.Purge();
            ManualCommands.Purge();
            FileCommands.Purge();
            MacroCommands.Purge();
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            if (e == ConnectionState.Online)
            {
                if (!_processing)
                    ProcessQueue();
                SystemCommands.StartProcessing();
                ManualCommands.StartProcessing();
                MacroCommands.StartProcessing();
            }
            else
            {
                _stopSubject.OnNext(Unit.Default);
                _processing = false;
                SystemCommands.PauseProcessing();
                ManualCommands.PauseProcessing();
                MacroCommands.PauseProcessing();

            }
        }

        private void OnCommandListUpdated()
        {
            CommandListUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnCommunicationLogUpdated()
        {
            CommunicationLogUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnResponseReceived(Response rsp)
        {
            ResponseReceived?.Invoke(this, rsp);
        }

        private void OnCommandFinished(Command cmd)
        {
            CommandFinished?.Invoke(this, cmd);
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            lock (_lockObject)
            {
                _uiContext.Send(
                    x =>
                        {
                            CommunicationLog.Insert(0, "grbl =>" + e);
                            if (CommunicationLog.Count > 500)
                            {
                                CommunicationLog.Remove(CommunicationLog.Last());
                            }
                        }, null);

                var type = _responseTypeFinder.GetType(e);

                OnResponseReceived(new Response { Data = e, Type = type });

                if ((type == ResponseType.Ok || type == ResponseType.Error || type == ResponseType.Alarm) && _waitingCommandQueue.Any() && _waitingCommandQueue.TryDequeue(out var cmd))
                {
                    CommandQueueLengthChanged?.Invoke(this, CommandQueueLength);
                    if (type == ResponseType.Ok)
                    {
                        cmd.ResultType = CommandResultType.Ok;
                    }
                    else if (type == ResponseType.Error)
                    {
                        cmd.ResultType = CommandResultType.Error;
                        cmd.CommandResultCause = e.Split(':')[1];

                        if (cmd.Source == CommandSourceType.File)
                        {
                            FileCommands.PauseProcessing();
                        }
                        else if (cmd.Source == CommandSourceType.Macros)
                        {
                            MacroCommands.Purge();
                        }
                    }
                    else if (type == ResponseType.Alarm)
                    {
                        cmd.ResultType = CommandResultType.Alarm;
                        cmd.CommandResultCause = e.Split(':')[1];

                        if (cmd.Source == CommandSourceType.File)
                        {
                            FileCommands.PauseProcessing();
                        }
                        else if (cmd.Source == CommandSourceType.Macros)
                        {
                            MacroCommands.Purge();
                        }
                    }

                    if (!string.IsNullOrEmpty(cmd.CommandOnResult))
                    {
                        SendAsync(cmd.CommandOnResult);
                    }

                    _uiContext.Send(
                    x =>
                        {
                            switch (cmd.Source)
                            {
                                case CommandSourceType.System:

                                    SystemCommands.CommandList.Insert(0, cmd);
                                    if (ManualCommands.CommandList.Count > 500)
                                    {
                                        ManualCommands.CommandList.Remove(ManualCommands.CommandList.Last());
                                    }
                                    break;
                                case CommandSourceType.Manual:

                                    ManualCommands.CommandList.Insert(0, cmd);
                                    if (ManualCommands.CommandList.Count > 500)
                                    {
                                        ManualCommands.CommandList.Remove(ManualCommands.CommandList.Last());
                                    }
                                    break;
                                case CommandSourceType.File:
                                    FileCommands.CommandList.Add(cmd);
                                    break;
                                case CommandSourceType.Macros:
                                    MacroCommands.CommandList.Add(cmd);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            OnCommandFinished(cmd);
                            OnCommandListUpdated();
                        },
                    null);
                }
                else if (_waitingCommandQueue.Any() && _waitingCommandQueue.TryPeek(out var peekCmd))
                {
                    if (peekCmd.ExpectedResponses.Any(x => x == type))
                    {
                        peekCmd.Result += (string.IsNullOrEmpty(peekCmd.Result) ? "" : Environment.NewLine) + e;
                    }
                }
            }
        }

        private void Send(Command cmd)
        {
            if (cmd == null)
                return;

            if (cmd.Type != RequestType.Realtime)
            {
                lock (_lockObject)
                {
                    _waitingCommandQueue.Enqueue(cmd);
                }
                CommandQueueLengthChanged?.Invoke(this, CommandQueueLength);

                _comService.Send(cmd.Data);
            }
            else
            {
                _comService.SendImmediate(cmd.Data);
            }
        }
    }
}