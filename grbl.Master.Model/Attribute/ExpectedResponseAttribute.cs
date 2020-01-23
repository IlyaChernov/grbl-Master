namespace grbl.Master.Model.Attribute
{
    using System;

    using grbl.Master.Model.Enum;

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
