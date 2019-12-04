namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;

    public class CommandSender : ICommandSender
    {
        private readonly ConcurrentQueue<Command> _commandQueue = new ConcurrentQueue<Command>();

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
                        if (_commandQueue.TryPeek(out var cmd) && cmd.Data.Length + _waitingCommandQueue.Select(x => x.Data.Length).Sum() <= _bufferSizeLimit && _commandQueue.TryDequeue(out var command))
                        {
                            Send(command);
                        }
                    });
        }

        public ObservableCollection<Command> CommandList { get; } = new ObservableCollection<Command>();

        public ObservableCollection<string> CommunicationLog { get; } = new ObservableCollection<string>();

        public event EventHandler<Response> ResponseReceived;

        public event EventHandler CommandListUpdated;

        public event EventHandler CommunicationLogUpdated;

        public event EventHandler<Command> CommandFinished;

        public void Send(string command)
        {
            lock (_lockObject)
            {
                _uiContext.Send(x => { CommunicationLog.Add("grbl <=" + command); }, null);
            }

            OnCommunicationLogUpdated();

            var cmd = new Command { Data = command };

            _commandPreProcessor.Process(ref cmd);

            if (cmd.Type != RequestType.Realtime)
            {
                lock (_lockObject)
                {
                    _commandQueue.Enqueue(cmd);
                }
            }
            else Send(cmd);
        }

        public void SendAsync(string command)
        {
            Observable.Start(() => { Send(command); }).Subscribe();
        }

        //public void SendAsync(byte[] command)
        //{
        //    Observable.Start(() => { Send(command); }).Subscribe();
        //}

        public void PurgeQueues()
        {
            while (_waitingCommandQueue.Any())
            {
                _waitingCommandQueue.TryDequeue(out var dummy);
            }
            while (_commandQueue.Any())
            {
                _commandQueue.TryDequeue(out var dummy);
            }
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            if (e == ConnectionState.Online)
            {
                if (!_processing)
                    ProcessQueue();
            }
            else
            {
                _stopSubject.OnNext(Unit.Default);
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

                OnResponseReceived(new Response{Data = e, Type = type});

                if ((type == ResponseType.Ok || type == ResponseType.Error) && _waitingCommandQueue.Any() && _waitingCommandQueue.TryDequeue(out var cmd))
                {
                    if (type == ResponseType.Ok)
                    {
                        cmd.ResultType = CommandResultType.Ok;
                    }
                    else if (type == ResponseType.Error)
                    {
                        cmd.ResultType = CommandResultType.Error;
                        cmd.CommandResultCause = e.Split(':')[1];
                    }

                    _uiContext.Send(
                        x =>
                            {
                                CommandList.Add(cmd);
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
                    else
                    {
                        var newCommand = new Command { Result = e };
                        _uiContext.Send(
                            x =>
                                {
                                    CommandList.Add(newCommand);
                                    OnCommandFinished(newCommand);
                                    OnCommandListUpdated();
                                },
                            null);
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