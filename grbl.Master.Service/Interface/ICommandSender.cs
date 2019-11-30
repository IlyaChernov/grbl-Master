namespace grbl.Master.Service.Interface
{
    using grbl.Master.Service.DataTypes;
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

        event EventHandler<Response> ResponseReceived;

        event EventHandler CommandListUpdated;

        event EventHandler CommunicationLogUpdated;

        event EventHandler<Command> CommandFinished;

        void Send(string command);

        void SendAsync(string command);

        void PurgeQueues();
    }
}