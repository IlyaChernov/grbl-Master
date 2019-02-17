namespace grbl.Master.Service.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;

    public interface ICommandSender
    {
        //List<Command> CommandList { get; }
        ObservableCollection<Command> CommandList { get; }

        event EventHandler CommandListUpdated;

        event EventHandler<Command> CommandFinished;

        void Send(string command, CommandType type);
    }
}