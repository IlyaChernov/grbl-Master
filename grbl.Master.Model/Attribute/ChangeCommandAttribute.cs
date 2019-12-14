namespace grbl.Master.Model.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public class ChangeCommandAttribute : Attribute
    {
        public string Comand { get; internal set; }

        public ChangeCommandAttribute(string command)
        {
            Comand = command;
        }
    }
}
