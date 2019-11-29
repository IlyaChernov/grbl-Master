using System;

namespace grbl.Master.Service.Enum
{
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
