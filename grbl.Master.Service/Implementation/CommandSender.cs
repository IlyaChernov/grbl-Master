namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.Annotations;
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class CommandSender : ICommandSender, INotifyPropertyChanged
    {
        private readonly IComService _comService;
        private int _currentIndex;

        private readonly object _lockObject = new object();

        private readonly SynchronizationContext _uiContext;

        public ObservableCollection<Command> CommandList { get; } = new ObservableCollection<Command>();

        public event EventHandler CommandListUpdated;

        public event EventHandler<Command> CommandFinished;

        private void OnCommandListUpdated()
        {
            CommandListUpdated?.Invoke(this, EventArgs.Empty);        
        }

        private void OnCommandFinished(Command cmd)
        {
            CommandFinished?.Invoke(this, cmd);
        }

        public CommandSender(IComService comService)
        {
            _uiContext = SynchronizationContext.Current;
            _comService = comService;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _comService.LineReceived += ComServiceLineReceived;
        }

        private void ComServiceConnectionStateChanged(object sender, EventArgs e)
        {
            if (_comService.IsConnected)
            {
                _currentIndex = 0;
                lock (_lockObject)
                    CommandList.Clear();
            }
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            lock (_lockObject)
                if (CommandList.Count > _currentIndex)
                {
                    if (e.Equals("ok"))
                    {
                        _uiContext.Send(
                            x =>
                                {
                                    CommandList[_currentIndex].ResultType = CommandResultType.Ok;
                                    OnCommandFinished(CommandList[_currentIndex]);
                                }, null);

                        _currentIndex++;
                    }
                    else if (e.StartsWith("ok"))
                    {
                        _uiContext.Send(x =>
                            {
                                CommandList[_currentIndex].ResultType = CommandResultType.Ok;
                                OnCommandFinished(CommandList[_currentIndex]);
                            }, null);

                        _currentIndex++;
                    }
                    else if (e.StartsWith("error"))
                    {
                        _uiContext.Send(x =>
                            {
                                CommandList[_currentIndex].ResultType = CommandResultType.Error;
                                CommandList[_currentIndex].CommandResultCause = e.Split(':')[1];
                                OnCommandFinished(CommandList[_currentIndex]);
                            }, null);

                        _currentIndex++;
                    }
                    else
                    {
                        _uiContext.Send(x =>
                            {
                                CommandList[_currentIndex].Result += e + Environment.NewLine;
                            }, null);
                    }
                }
                else
                {
                    _uiContext.Send(x => CommandList.Add(new Command { Result = e }), null);
                    _currentIndex++;
                }

            OnCommandListUpdated();
        }

        public void Send(string command, CommandType type)
        {
            _uiContext.Send(x => CommandList.Add(new Command { Data = command, Type = type }), null);
            _comService.Send(command);
            OnCommandListUpdated();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
