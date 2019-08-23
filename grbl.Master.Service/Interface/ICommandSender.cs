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

        ObservableCollection<string> CommunicationLog
        {
            get;
        }

        event EventHandler CommandListUpdated;

        event EventHandler CommunicationLogUpdated;

        event EventHandler<Command> CommandFinished;

        void Send(string command, CommandType type);

        void SendSystem(string command);

        void SendRealtime(char command);

        void PurgeQueues();
    }
}