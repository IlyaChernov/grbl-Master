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

        private readonly object _lockObject = new object();

        private bool _processing;

        private readonly char[] _realtimeCommands =
            {
                // Basic
                '\x18', // (ctrl-x) : Soft-Reset
                '?', // Status Report Query
                '~', // Cycle Start / Resume
                '!', // Feed Hold

                // Extended
                '\x84', // Safety door
                '\x85', // Jog Cancel

                // Feed Overrides
                '\x90', // Set 100% of programmed rate.
                '\x91', // Increase 10%
                '\x92', // Decrease 10%
                '\x93', // Increase 1%
                '\x94', // Decrease 1%

                // Rapid Overrides
                '\x95', // Set to 100% full rapid rate.
                '\x96', // Set to 50% of rapid rate.
                '\x97', // Set to 25% of rapid rate.

                // Spindle Speed Overrides
                '\x99', // Set 100% of programmed spindle speed
                '\x9A', // Increase 10%
                '\x9B', // Decrease 10%
                '\x9C', // Increase 1%
                '\x9D', // Decrease 1%

                //Toggles
                '\x9E', //Toggle Spindle Stop
                '\xA0', //Toggle Flood Coolant
                '\xA1' //Toggle Mist Coolant
            };
        
        private readonly SynchronizationContext _uiContext;

        readonly Subject<Unit> _stopSubject = new Subject<Unit>();

        private readonly ConcurrentQueue<Command> _waitingCommandQueue = new ConcurrentQueue<Command>();

        private int _bufferSizeLimit = 100;

        public CommandSender(IComService comService)
        {
            _uiContext = SynchronizationContext.Current;
            _comService = comService;
            _comService.LineReceived += ComServiceLineReceived;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
        }

        private void ProcessQueue()
        {
            _processing = true;
            Observable.Timer(TimeSpan.Zero, TimeSpan.Zero).TakeUntil(_stopSubject).Subscribe(
                l =>
                    {
                        //Command cmd;
                        if (_commandQueue.TryPeek(out var cmd) && cmd.Data.Length + _waitingCommandQueue.Select(x => x.Data.Length).Sum() <= _bufferSizeLimit && _commandQueue.TryDequeue(out var command))
                        {
                            Send(command);
                        }

                        //if (_commandQueue.Any() && _commandQueue.Peek().Data.Length
                        //    + _waitingCommandQueue.Select(x => x.Data.Length).Sum() <= _bufferSizeLimit)
                        //{
                        //    lock (_lockObject)
                        //    {
                        //        Send(_commandQueue.Dequeue());
                        //    }
                        //}
                    });
        }

        public ObservableCollection<Command> CommandList { get; } = new ObservableCollection<Command>();

        public ObservableCollection<string> CommunicationLog { get; } = new ObservableCollection<string>();

        public event EventHandler CommandListUpdated;

        public event EventHandler CommunicationLogUpdated;

        public event EventHandler<Command> CommandFinished;

        public void Send(string command, CommandType type)
        {
            var realtimeOverride = type != CommandType.Realtime && command.Length == 1
                                                                && _realtimeCommands.Any(x => x == command[0]);

            lock (_lockObject)
            {
                _uiContext.Send(x => { CommunicationLog.Add("grbl <=" + command); }, null);
            }

            OnCommunicationLogUpdated();

            var cmd = new Command { Data = command, Type = realtimeOverride ? CommandType.Realtime : type };

            if (cmd.Type != CommandType.Realtime)
            {
                lock (_lockObject)
                {
                    _commandQueue.Enqueue(cmd);
                }
            }
            else Send(cmd);
        }

        public void SendGCode(string command)
        {
            Observable.Start(() => { Send(command, CommandType.GCode); }).Subscribe();
        }

        public void SendSystem(string command)
        {
            Observable.Start(() => { Send(command, CommandType.System); }).Subscribe();
        }

        public void SendRealtime(string command)
        {
            Observable.Start(() => { Send(command, CommandType.Realtime); }).Subscribe();
        }

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
            //_waitingCommandQueue.Clear();
            //_commandQueue.Clear();
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

        private void OnCommandFinished(Command cmd)
        {
            CommandFinished?.Invoke(this, cmd);
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            lock (_lockObject)
            {
                _uiContext.Send(x => { CommunicationLog.Add("grbl =>" + e); }, null);

                if ((e.StartsWith("ok") || e.StartsWith("error")) && _waitingCommandQueue.Any() && _waitingCommandQueue.TryDequeue(out var cmd)) // _waitingCommandQueue.Any())
                {
                    //var cmd = _waitingCommandQueue.Dequeue();
                    //_waitingCommandQueue.TryDequeue(out var cmd);
                    
                    if (e.StartsWith("ok"))
                    {
                        cmd.ResultType = CommandResultType.Ok;
                    }
                    else if (e.StartsWith("error"))
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
                else if (_waitingCommandQueue.Any() && this._waitingCommandQueue.TryPeek(out var comd))
                {
                    //var cmd = _waitingCommandQueue.Peek();

                    comd.Result += (string.IsNullOrEmpty(comd.Result) ? "" : Environment.NewLine) + e;
                }
            }
        }

        private void Send(Command cmd)
        {
            if (cmd == null)
                return;

            if (cmd.Type != CommandType.Realtime)
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