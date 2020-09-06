namespace grbl.Master.Model.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public class ChangeCommandAttribute : Attribute
    {
        public string Command { get; internal set; }

        public ChangeCommandAttribute(string command)
        {
            this.Command = command;
        }
    }
}
