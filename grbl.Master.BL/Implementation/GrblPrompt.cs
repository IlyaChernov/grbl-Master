namespace grbl.Master.BL.Implementation
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Text.RegularExpressions;

    public class GrblPrompt : IGrblPrompt
    {
        private const string PromptTag = "^Grbl.{6}\\[.*\\]$";

        readonly Regex _promptReg = new Regex(PromptTag);

        public GrblPrompt(IComService comService)
        {
            comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            comService.LineReceived += ComServiceLineReceived;
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            if (Received || string.IsNullOrEmpty(e))
            {
                return;
            }

            if (_promptReg.IsMatch(e))
            {
                Received = true;
                Message = e;
                OnPromptReceived(Message);
            }
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            if (e == ConnectionState.Offline)
            {
                Received = false;
                Message = string.Empty;
            }
        }

        public event EventHandler<string> PromptReceived;

        private void OnPromptReceived(string data)
        {
            PromptReceived?.Invoke(this, data);
        }

        public bool Received
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }
    }
}
