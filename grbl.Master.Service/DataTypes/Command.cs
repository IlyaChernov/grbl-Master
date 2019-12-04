namespace grbl.Master.Service.DataTypes
{
    using System.Collections.Generic;

    using grbl.Master.Service.Annotations;
    using grbl.Master.Service.Enum;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Command that will be stored in memory after sending. Can be updated with results.
    /// </summary>
    public class Command : INotifyPropertyChanged
    {
        private string _data;

        private RequestType? _type;

        public List<ResponseType> ExpectedResponses { get; set; }

        private string _result;

        private CommandResultType? _resultType;

        private string _commandResultCause;

        /// <summary>
        /// Data sent
        /// </summary>
        public string Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Type of command
        /// </summary>
        public RequestType? Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Answer data
        /// </summary>
        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Answer type
        /// </summary>
        public CommandResultType? ResultType
        {
            get => _resultType;
            set
            {
                _resultType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Cause of result
        /// </summary>
        public string CommandResultCause
        {
            get => _commandResultCause;
            set
            {
                _commandResultCause = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
