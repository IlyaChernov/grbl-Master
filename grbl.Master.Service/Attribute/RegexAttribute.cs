namespace grbl.Master.Service.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public class RegexAttribute : Attribute
    {
        public string RegularExpression { get; internal set; }

        public RegexAttribute(string regex)
        {
            RegularExpression = regex;
        }
    }
}
