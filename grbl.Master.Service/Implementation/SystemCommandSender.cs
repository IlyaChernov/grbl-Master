﻿namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class StatusCommandSender
    {
        private readonly ICommandSender _commandSender;

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

        private void CommandSenderCommandListUpdated(object sender, EventArgs e)
        {
            OnCommandListUpdated();
        }

        public StatusCommandSender(ICommandSender commandSender)
        {
            _commandSender = commandSender;
            _commandSender.CommandListUpdated += CommandSenderCommandListUpdated;
            _commandSender.CommandFinished += CommandSenderCommandFinished;
        }

        private void CommandSenderCommandFinished(object sender, Command e)
        {
            OnCommandFinished(e);
        }

        public ObservableCollection<Command> CommandList => _commandSender.CommandList;

        public void Send(string command)
        {
            _commandSender.Send(command, CommandType.StatusRequest);
        }
    }
}