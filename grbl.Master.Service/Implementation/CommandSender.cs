namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;

    using grbl.Master.Model;
    using grbl.Master.Model.Enum;

    public class CommandSender : ICommandSender
    {

        private readonly IComService _comService;

        private readonly IGrblResponseTypeFinder _responseTypeFinder;

        private readonly IGrblCommandPreProcessor _commandPreProcessor;

        private readonly object _lockObject = new object();

        private bool _processing;

        private readonly SynchronizationContext _uiContext;

        readonly Subject<Unit> _stopSubject = new Subject<Unit>();

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
            Observable.Timer(TimeSpan.Zero, TimeSpan.Zero).TakeUntil(_stopSubject).Subscribe(
                l =>
                    {
                        //Debug.WriteLine("Peek");

                        if ((SystemCommands.TryPeekCommand(out var cmd) || ManualCommands.TryPeekCommand(out cmd) || FileCommands.TryPeekCommand(out cmd)) && cmd != null && cmd.Data.Length + _waitingCommandQueue.Sum(x => x.Data.Length) <= _bufferSizeLimit)
                        {
                            Debug.WriteLine("Peeking successful");
                            var continueProcess = false;
                            switch (cmd.Source)
                            {
                                case CommandSourceType.System:
                                    continueProcess = SystemCommands.TryGetCommand(out cmd);
                                    break;
                                case CommandSourceType.Manual:
                                    continueProcess = ManualCommands.TryGetCommand(out cmd);
                                    break;
                                case CommandSourceType.File:
                                    continueProcess = FileCommands.TryGetCommand(out cmd);
                                    break;
                            }

                            if (!continueProcess) return;

                            _commandPreProcessor.Process(ref cmd);

                            Send(cmd);
                        }

                        if (cmd != null)
                        {
                            Debug.WriteLine($"Current buffer length {cmd.Data.Length + _waitingCommandQueue.Sum(x => x.Data.Length)} , {_waitingCommandQueue.Aggregate("",  (prev, next) => prev + next.Data )}");
                        }
                    });
        }

        public CommandSource SystemCommands { get; } = new CommandSource(CommandSourceType.System, CommandSourceRunMode.Infinite);

        public CommandSource ManualCommands { get; } = new CommandSource(CommandSourceType.Manual, CommandSourceRunMode.Infinite);

        public CommandSource FileCommands { get; } = new CommandSource(CommandSourceType.File, CommandSourceRunMode.StopInTheEnd);

        public ObservableCollection<string> CommunicationLog { get; } = new ObservableCollection<string>();

        public event EventHandler<Response> ResponseReceived;

        public event EventHandler CommandListUpdated;

        public event EventHandler CommunicationLogUpdated;

        public event EventHandler<Command> CommandFinished;

        public void Send(string command, string onResult = null)
        {
            lock (_lockObject)
            {
                _uiContext.Send(x => { CommunicationLog.Add("grbl <=" + command); }, null);
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
                            SystemCommands.Add(command);
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

        public Command Prepare(string command)
        {
            var cmd = new Command { Data = command };

            _commandPreProcessor.Process(ref cmd);

            return cmd;
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
            }

            SystemCommands.Purge();
            ManualCommands.Purge();
            FileCommands.Purge();
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            if (e == ConnectionState.Online)
            {
                if (!_processing)
                    ProcessQueue();
                SystemCommands.StartProcessing();
                ManualCommands.StartProcessing();
            }
            else
            {
                _stopSubject.OnNext(Unit.Default);
                SystemCommands.PauseProcessing();
                ManualCommands.PauseProcessing();

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
                _uiContext.Send(x => { CommunicationLog.Add("grbl =>" + e); }, null);

                var type = _responseTypeFinder.GetType(e);

                OnResponseReceived(new Response { Data = e, Type = type });

                if ((type == ResponseType.Ok || type == ResponseType.Error || type == ResponseType.Alarm) && _waitingCommandQueue.Any() && _waitingCommandQueue.TryDequeue(out var cmd))
                {
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
                    }
                    else if(type == ResponseType.Alarm)
                    {
                        cmd.ResultType = CommandResultType.Alarm;
                        cmd.CommandResultCause = e.Split(':')[1];

                        if (cmd.Source == CommandSourceType.File)
                        {
                            FileCommands.PauseProcessing();
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
                                    SystemCommands.CommandList.Add(cmd);
                                    break;
                                case CommandSourceType.Manual:
                                    ManualCommands.CommandList.Add(cmd);
                                    break;
                                case CommandSourceType.File:
                                    FileCommands.CommandList.Add(cmd);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            OnCommandFinished(cmd);
                            OnCommandListUpdated();
                        },
                    null);
                }
                else if (_waitingCommandQueue.Any() && _waitingCommandQueue.TryPeek(out var comd))
                {
                    if (comd.ExpectedResponses.Any(x => x == type))
                    {
                        comd.Result += (string.IsNullOrEmpty(comd.Result) ? "" : Environment.NewLine) + e;
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

                _comService.Send(cmd.Data);
            }
            else
            {
                _comService.SendImmediate(cmd.Data);
            }
        }
    }
}