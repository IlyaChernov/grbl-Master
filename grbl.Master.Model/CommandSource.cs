namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;
    using PropertyChanged;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    public class CommandSource : NotifyPropertyChanged
    {
        private readonly Stopwatch _stopWatch = new Stopwatch();

        private bool _needsPurge;

        private readonly SynchronizationContext _uiContext;

        private ConcurrentQueue<string> CommandQueue { get; } = new ConcurrentQueue<string>();

        public TimeSpan Elapsed => _stopWatch.Elapsed;

        [DependsOn(nameof(CommandQueue))]
        public int CommandCount => CommandQueue.Count;

        public CommandSourceState State { get; internal set; } = CommandSourceState.Stopped;

        public CommandSourceRunMode Mode { get; set; }

        public CommandSourceType Type { get; set; }

        public ObservableCollection<Command> CommandList { get; } = new ObservableCollection<Command>();

        public CommandSource(CommandSourceType type, CommandSourceRunMode mode)
        {
            _uiContext = SynchronizationContext.Current;
            Type = type;
            Mode = mode;
        }

        public void StartProcessing()
        {
            if (State == CommandSourceState.Stopped && _needsPurge)
            {
                Purge();
            }

            _stopWatch.Start();

            State = CommandSourceState.Running;
        }


        public void PauseProcessing()
        {
            if (State == CommandSourceState.Running)
            {
                State = CommandSourceState.Paused;
                _stopWatch.Stop();
            }
        }

        public void StopProcessing()
        {
            _needsPurge = true;
            State = CommandSourceState.Stopped;
            _stopWatch.Stop();
        }

        public bool TryPeekCommand(out Command command)
        {
            if (!CommandQueue.IsEmpty)
            {
                if (State == CommandSourceState.Running && CommandQueue.TryPeek(out var cmdText))
                {
                    command = new Command { Data = cmdText, Source = Type };
                    return true;
                }
            }

            command = null;
            return false;
        }

        public bool TryGetCommand(out Command command)
        {
            if (State == CommandSourceState.Running && CommandQueue.TryDequeue(out var cmdText))
            {
                command = new Command { Data = cmdText, Source = Type };
                if (Mode == CommandSourceRunMode.LineByLine)
                {
                    PauseProcessing();
                }

                if (Mode == CommandSourceRunMode.StopInTheEnd && CommandQueue.IsEmpty)
                {
                    StopProcessing();
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

            _uiContext.Send(
                state =>
                    {
                        CommandList.Clear();
                    }, null);

            _stopWatch.Reset();
        }

        public void Add(string command)
        {
            Add(command.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
        }

        public void Add(string[] commands)
        {
            foreach (var command in commands)
            {
                CommandQueue.Enqueue(command);
            }
        }
    }
}
