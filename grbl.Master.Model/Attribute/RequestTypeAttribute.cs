namespace grbl.Master.Model.Attribute
{
    using System;

    using grbl.Master.Model.Enum;

    [AttributeUsage(AttributeTargets.All)]
    public class RequestTypeAttribute : Attribute
    {
        public RequestType Type { get; internal set; }

        public RequestTypeAttribute(RequestType type)
        {
            Type = type;
        }
    }
}
