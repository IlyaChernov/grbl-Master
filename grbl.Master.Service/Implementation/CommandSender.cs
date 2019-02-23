namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    public class CommandSender : ICommandSender
    {
        private readonly char[] _realtimeCommands =
            {
                // Basic
                '\x18', // (ctrl-x) : Soft-Reset
                '?',    // Status Report Query
                '~',    // Cycle Start / Resume
                '!',    // Feed Hold

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

        private readonly Queue<Command> _commandQueue = new Queue<Command>();

        private readonly IComService _comService;

        private readonly object _lockObject = new object();

        private readonly SynchronizationContext _uiContext;

        private readonly Queue<Command> _waitingCommandQueue = new Queue<Command>();

        private bool _realtimeWaiting;

        public CommandSender(IComService comService)
        {
            _uiContext = SynchronizationContext.Current;
            _comService = comService;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _comService.LineReceived += ComServiceLineReceived;
        }

        public ObservableCollection<Command> CommandList { get; } = new ObservableCollection<Command>();

        public ObservableCollection<string> CommunicationLog { get; } = new ObservableCollection<string>();

        public event EventHandler CommandListUpdated;

        public event EventHandler CommunicationLogUpdated;

        public event EventHandler<Command> CommandFinished;

        public void Send(string command, CommandType type)
        {
            var realtimeOverride = (type != CommandType.Realtime && command.Length == 1 && _realtimeCommands.Any(x => x == command[0]));

            _uiContext.Send(x => { CommunicationLog.Add("grbl <=" + command); }, null);

            OnCommunicationLogUpdated();

            var cmd = new Command { Data = command, Type = realtimeOverride ? CommandType.Realtime : type };

            if (type != CommandType.Realtime && _waitingCommandQueue.Any() || _realtimeWaiting)
                _commandQueue.Enqueue(cmd);
            else Send(cmd);
        }

        public void SendSystem(string command)
        {
            Send(command, CommandType.System);
        }

        public void SendRealtime(char command)
        {
            Send("" + command, CommandType.Realtime);
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

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            if (e == ConnectionState.Online)
                lock (_lockObject)
                {
                    CommandList.Clear();
                }
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            lock (_lockObject)
            {
                _uiContext.Send(x => { CommunicationLog.Add("grbl =>" + e); }, null);
            }

            if (_realtimeWaiting)
            {
                if (e.StartsWith("ok") || e.StartsWith("error"))
                {
                    _realtimeWaiting = false;
                    if (_commandQueue.Any()) Send(_commandQueue.Dequeue());
                }
            }
            else
            {
                lock (_lockObject)
                {
                    if (e.StartsWith("ok") || e.StartsWith("error"))
                    {
                        var cmd = _waitingCommandQueue.Dequeue();

                        if (_commandQueue.Any()) Send(_commandQueue.Dequeue());

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
                    else if (_waitingCommandQueue.Any())
                    {
                        _waitingCommandQueue.Peek().Result += e;
                    }
                }
            }
        }

        private void Send(Command cmd)
        {
            if (cmd.Type != CommandType.Realtime) _waitingCommandQueue.Enqueue(cmd);
            else _realtimeWaiting = true;

            _comService.Send(cmd.Data);
        }
    }
}