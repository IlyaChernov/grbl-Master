namespace grbl.Master.Service.DataTypes
{
    using grbl.Master.Service.Enum;

    /// <summary>
    /// Command that will be stored in memory after sending. Can be updated with results.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Data sent
        /// </summary>
        public string Data
        {
            get; set;
        }

        /// <summary>
        /// Type of command
        /// </summary>
        public CommandType? Type
        {
            get; set;
        }

        /// <summary>
        /// Answer data
        /// </summary>
        public string Result
        {
            get; set;
        }

        /// <summary>
        /// Answer type
        /// </summary>
        public CommandResultType? ResultType
        {
            get; set;
        }

        /// <summary>
        /// Cause of result
        /// </summary>
        public string CommandResultCause
        {
            get; set;
        }
    }
}
