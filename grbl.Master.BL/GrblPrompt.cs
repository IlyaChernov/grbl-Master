namespace grbl.Master.BL
{
    using System;
    using System.Text.RegularExpressions;

    using grbl.Master.Common.Interfaces.BL;
    using grbl.Master.Common.Interfaces.Service;

    public class GrblPrompt : IGrblPrompt
    {
        private const string PromptTag = "^Grbl.{6}\\[.*\\]$";

        private readonly Regex _promptReg = new Regex(PromptTag);

        public GrblPrompt(IComService comService)
        {
            comService.LineReceived += ComServiceLineReceived;
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            if (_promptReg.IsMatch(e))
            {
                OnPromptReceived(e);
            }
        }

        public event EventHandler<string> PromptReceived;

        private void OnPromptReceived(string data)
        {
            PromptReceived?.Invoke(this, data);
        }
    }
}
