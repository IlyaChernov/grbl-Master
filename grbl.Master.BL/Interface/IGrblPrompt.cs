namespace grbl.Master.BL.Interface
{
    using System;

    public interface IGrblPrompt
    {
        event EventHandler<string> PromptReceived;

        bool Received
        {
            get;
        }

        string Message
        {
            get;
        }
    }
}