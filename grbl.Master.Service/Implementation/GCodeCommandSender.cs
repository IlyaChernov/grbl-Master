namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class GCodeCommandSender
    {
        private readonly ICommandSender _commandSender;

        public event EventHandler CommandListUpdated;

        private void OnCommandListUpdated()
        {
            CommandListUpdated?.Invoke(this, EventArgs.Empty);
        }

        public GCodeCommandSender(ICommandSender commandSender)
        {
            _commandSender = commandSender;
            _commandSender.CommandListUpdated += CommandSenderCommandListUpdated;
        }

        private void CommandSenderCommandListUpdated(object sender, EventArgs e)
        {
            OnCommandListUpdated();
        }

        public ObservableCollection<Command> CommandList => _commandSender.CommandList;

        public void Send(string command)
        {
            _commandSender.Send(command, CommandType.GCode);
        }
    }
}
