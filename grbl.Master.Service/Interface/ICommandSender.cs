namespace grbl.Master.Service.Interface
{
    using System;
    using System.Collections.ObjectModel;

    using grbl.Master.Model;

    public interface ICommandSender
    {
        CommandSource SystemCommands { get; }
         CommandSource ManualCommands { get; }
        CommandSource FileCommands { get; }

        ObservableCollection<string> CommunicationLog
        {
            get;
        }

        event EventHandler<Response> ResponseReceived;

        event EventHandler CommunicationLogUpdated;

        void SendAsync(string command, string onResult = null);

        void Send(string command, string onResult = null);

        Command Prepare(string command);

        void PurgeQueues();
    }
}