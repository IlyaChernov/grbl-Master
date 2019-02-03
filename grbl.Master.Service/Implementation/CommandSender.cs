namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Generic;

    public class CommandSender : ICommandSender
    {
        private readonly IComService _comService;

        private readonly List<Command> _commandList = new List<Command>();

        private int _currentIndex;

        public List<Command> CommandList => _commandList;

        public event EventHandler CommandListUpdated;

        private void OnCommandListUpdated()
        {
            CommandListUpdated?.Invoke(this, EventArgs.Empty);
        }

        public CommandSender(IComService comService)
        {
            _comService = comService;
            _comService.LineReceived += ComServiceLineReceived;
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            if (_commandList.Count > _currentIndex)
            {
                if (e.Equals("ok"))
                {
                    _commandList[_currentIndex].ResultType = CommandResultType.Ok;
                    _currentIndex++;
                }
                else if (e.StartsWith("error"))
                {
                    _commandList[_currentIndex].ResultType = CommandResultType.Error;
                    _commandList[_currentIndex].CommandResultCause = e.Split(':')[1];
                    _currentIndex++;
                }
                else
                {
                    _commandList[_currentIndex].Result = e;
                }
            }
            else
            {
                _commandList.Add(new Command { Result = e });
                _currentIndex++;
            }

            OnCommandListUpdated();
        }

        public void Send(string command, CommandType type)
        {
            _commandList.Add(new Command { Data = command, Type = type });
            _comService.Send(command);
            OnCommandListUpdated();
        }
    }
}
