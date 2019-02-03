namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Generic;

    public class SystemCommandSender
    {
        private readonly ICommandSender _commandSender;

        public event EventHandler CommandListUpdated;

        private void OnCommandListUpdated()
        {
            CommandListUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void CommandSenderCommandListUpdated(object sender, EventArgs e)
        {
            OnCommandListUpdated();
        }

        public SystemCommandSender(ICommandSender commandSender)
        {
            _commandSender = commandSender;
            _commandSender.CommandListUpdated += CommandSenderCommandListUpdated;
        }

        public List<Command> CommandList => _commandSender.CommandList;

        public void Send(string command)
        {
            _commandSender.Send(command, CommandType.System);
        }
    }
}
