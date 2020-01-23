namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;

    /// <summary>
    /// Response that will be processed after receiving
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Data received
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Type of response
        /// </summary>
        public ResponseType Type { get; set; }
    }
}
