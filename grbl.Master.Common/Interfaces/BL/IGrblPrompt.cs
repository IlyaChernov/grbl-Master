namespace grbl.Master.Common.Interfaces.BL
{
    using System;

    public interface IGrblPrompt
    {
        event EventHandler<string> PromptReceived;
    }
}