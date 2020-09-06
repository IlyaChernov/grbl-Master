namespace grbl.Master.Common.Interfaces.Service
{
    using System;
    using System.Collections.ObjectModel;

    using grbl.Master.Model;

    public interface ICommandSender
    {
        event EventHandler<int> CommandQueueLengthChanged;

        CommandSource SystemCommands { get; }
        CommandSource ManualCommands { get; }
        CommandSource MacroCommands { get; }
        CommandSource FileCommands { get; }

        int CommandQueueLength { get; }

        ObservableCollection<string> CommunicationLog
        {
            get;
        }

        event EventHandler<Response> ResponseReceived;

        event EventHandler CommunicationLogUpdated;

        void SendAsync(string command, string onResult = null);

        void Send(string command, string onResult = null);

        //Command Prepare(string command);

        void PurgeQueues();
    }
}