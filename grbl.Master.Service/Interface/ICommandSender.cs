namespace grbl.Master.Service.Interface
{
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using System;
    using System.Collections.ObjectModel;

    public interface ICommandSender
    {
        ObservableCollection<Command> CommandList
        {
            get;
        }

        event EventHandler CommandListUpdated;

        event EventHandler<Command> CommandFinished;

        void Send(string command, CommandType type);

        void Send(char command, CommandType type);
    }
}