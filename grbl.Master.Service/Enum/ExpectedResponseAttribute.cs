using System;

namespace grbl.Master.Service.Enum
{
    [AttributeUsage(AttributeTargets.All)]
    public class ExpectedResponseAttribute : Attribute
    {
        public ResponseType[] ResponseTypes { get; internal set; }

        public ExpectedResponseAttribute(params ResponseType[] types)
        {
            ResponseTypes = types;
        }
    }
}
