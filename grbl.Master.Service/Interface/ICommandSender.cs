namespace grbl.Master.Service.Interface
{
    using System;
    using System.Collections.Generic;

    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;

    public interface ICommandSender
    {
        List<Command> CommandList { get; }

        event EventHandler CommandListUpdated;

        void Send(string command, CommandType type);
    }
}