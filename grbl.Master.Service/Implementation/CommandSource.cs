using System;

namespace grbl.Master.Service.Implementation
{
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;

    public class CommandSource : ICommandSource
    {
        private readonly object _lockObject = new object();

        private bool _needsPurge;
        private ConcurrentQueue<string> CommandQueue { get; } = new ConcurrentQueue<string>();

        public CommandSourceState State { get; internal set; } = CommandSourceState.Stopped;

        public CommandSourceType Type { get; set; }

        public ObservableCollection<Command> CommandList { get; } = new ObservableCollection<Command>();

        public event EventHandler CommandListUpdated;

        public event EventHandler<Command> CommandFinished;

        public CommandSource(CommandSourceType type)
        {
            Type = type;
        }

        public void StartProcessing()
        {
            if (State == CommandSourceState.Stopped && _needsPurge)
            {
                Purge();
            }

            State = CommandSourceState.Running;
        }

        public void StartLineByLineProcessing()
        {
            if (State == CommandSourceState.Stopped && _needsPurge)
            {
                Purge();
            }

            State = CommandSourceState.RunningLineByLine;
        }

        public void PauseProcessing()
        {
            if (State == CommandSourceState.Running)
            {
                State = CommandSourceState.Paused;
            }
        }

        public void StopProcessing()
        {
            _needsPurge = true;
            State = CommandSourceState.Stopped;
        }

        public bool TryPeekCommand(out Command command)
        {
            if ((State == CommandSourceState.Running || State == CommandSourceState.RunningLineByLine) && CommandQueue.TryPeek(out var cmdText))
            {
                command = new Command { Data = cmdText, Source = Type };
                return true;
            }

            command = null;
            return false;
        }

        public bool TryGetCommand(out Command command)
        {
            if ((State == CommandSourceState.Running || State== CommandSourceState.RunningLineByLine) && CommandQueue.TryDequeue(out var cmdText))
            {
                command = new Command { Data = cmdText, Source = Type };
                if (State == CommandSourceState.RunningLineByLine)
                {
                    State = CommandSourceState.Paused;
                }
                return true;
            }

            command = null;
            return false;
        }

        public void Purge()
        {
            _needsPurge = false;
            while (CommandQueue.Any())
            {
                CommandQueue.TryDequeue(out var dummy);
            }
            CommandList.Clear();
        }

        public void Add(string command)
        {
            Debug.WriteLine($"Command {command} added to {Type}");
            lock (_lockObject)
            {
                CommandQueue.Enqueue(command);
            }
        }

        public void Add(string[] commands)
        {
            foreach (var command in commands)
            {
                lock (_lockObject)
                {
                    CommandQueue.Enqueue(command);
                }
            }
        }
    }
}
