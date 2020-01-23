namespace grbl.Master.Model
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Linq;

    using grbl.Master.Model.Enum;

    public class CommandSource
    {
        //private readonly object _lockObject = new object();

        private bool _needsPurge;
        private ConcurrentQueue<string> CommandQueue { get; } = new ConcurrentQueue<string>();

        public int CommandCount => CommandQueue.Count;

        public CommandSourceState State { get; internal set; } = CommandSourceState.Stopped;

        public CommandSourceRunMode Mode { get; set; }

        public CommandSourceType Type { get; set; }

        public ObservableCollection<Command> CommandList { get; } = new ObservableCollection<Command>();

        public event EventHandler CommandListUpdated;

        public event EventHandler<Command> CommandFinished;

        public CommandSource(CommandSourceType type, CommandSourceRunMode mode)
        {
            Type = type;
            Mode = mode;
        }

        public void StartProcessing()
        {
            if (State == CommandSourceState.Stopped && _needsPurge)
            {
                Purge();
            }

            State = CommandSourceState.Running;
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
            if (State == CommandSourceState.Running && CommandQueue.TryPeek(out var cmdText))
            {
                command = new Command { Data = cmdText, Source = Type };
                return true;
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
                    this.PauseProcessing();
                }

                if (Mode == CommandSourceRunMode.StopInTheEnd && CommandQueue.IsEmpty)
                {
                    this.StopProcessing();
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
            Add(command.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
            //Debug.WriteLine($"Command {command} added to {Type}");
           // lock (_lockObject)
           // {
                //CommandQueue.Enqueue(command);
           // }
        }

        public void Add(string[] commands)
        {
            foreach (var command in commands)
            {
                //lock (_lockObject)
                //{
                    CommandQueue.Enqueue(command);
                //}
            }
        }
    }
}
