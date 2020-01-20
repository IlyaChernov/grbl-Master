namespace grbl.Master.Service.Interface
{
    using System;
    using System.Collections.ObjectModel;

    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;

    interface ICommandSource
    {
        CommandSourceState State { get; }

        CommandSourceType Type { get; set; }

        ObservableCollection<Command> CommandList
        {
            get;
        }

        event EventHandler CommandListUpdated;

        event EventHandler<Command> CommandFinished;

        void StartProcessing();

        void StartLineByLineProcessing();

        void PauseProcessing();

        void StopProcessing();

        bool TryPeekCommand(out Command command);

        bool TryGetCommand(out Command command);

        void Purge();

        void Add(string command);
    }
}
