namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;
    using System.Collections.Generic;

    /// <summary>
    /// Command that will be stored in memory after sending. Can be updated with results.
    /// </summary>
    public class Command : NotifyPropertyChanged
    {
        public List<ResponseType> ExpectedResponses { get; set; }

        public string CommandOnResult { get; set; }

        /// <summary>
        /// Data sent
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Type of command
        /// </summary>
        public RequestType? Type { get; set; }

        /// <summary>
        /// Answer data
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Answer type
        /// </summary>
        public CommandResultType? ResultType { get; set; }

        /// <summary>
        /// Cause of result
        /// </summary>
        public string CommandResultCause { get; set; }

        public CommandSourceType Source { get; set; }
    }
}
