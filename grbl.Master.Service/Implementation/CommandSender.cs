namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;

    public class CommandSender : ICommandSender
    {
        private readonly IComService _comService;

        //private readonly List<Command> _commandList = new List<Command>();        
        private readonly ObservableCollection<Command> _commandList = new ObservableCollection<Command>();

        private int _currentIndex;

        private object _lockObject = new object();

        private SynchronizationContext _uiContext;

        //public List<Command> CommandList => _commandList;

        public ObservableCollection<Command> CommandList => _commandList;

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

            //BindingOperations.EnableCollectionSynchronization(_commandList, _lockObject);
        }

        private void ComServiceConnectionStateChanged(object sender, EventArgs e)
        {
            if (_comService.IsConnected)
            {
                _currentIndex = 0;
                lock (_lockObject)
                    _commandList.Clear();
            }
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            lock (_lockObject)
                if (_commandList.Count > _currentIndex)
                {
                    if (e.Equals("ok"))
                    {
                        _uiContext.Send(
                            x =>
                                {
                                    _commandList[_currentIndex].ResultType = CommandResultType.Ok;
                                    OnCommandFinished(_commandList[_currentIndex]);
                                }, null);

                        _currentIndex++;
                    }
                    else if (e.StartsWith("ok"))
                    {
                        _uiContext.Send(x =>
                            {
                                _commandList[_currentIndex].ResultType = CommandResultType.Ok;
                                OnCommandFinished(_commandList[_currentIndex]);
                            }, null);

                        _currentIndex++;
                    }
                    else if (e.StartsWith("error"))
                    {
                        _uiContext.Send(x =>
                            {
                                _commandList[_currentIndex].ResultType = CommandResultType.Error;
                                _commandList[_currentIndex].CommandResultCause = e.Split(':')[1];
                                OnCommandFinished(_commandList[_currentIndex]);
                            }, null);

                        _currentIndex++;
                    }
                    else
                    {
                        _uiContext.Send(x =>
                            {
                                _commandList[_currentIndex].Result += e + Environment.NewLine;
                            }, null);
                    }
                }
                else
                {
                    _uiContext.Send(x => _commandList.Add(new Command { Result = e }), null);
                    //_commandList.Add(new Command { Result = e });
                    _currentIndex++;
                }

            OnCommandListUpdated();
        }

        public void Send(string command, CommandType type)
        {
            _uiContext.Send(x => _commandList.Add(new Command { Data = command, Type = type }), null);
            //_commandList.Add(new Command { Data = command, Type = type });
            _comService.Send(command);
            OnCommandListUpdated();
        }
    }
}
