using System;

namespace grbl.Master.Service.Enum
{
    [AttributeUsage(AttributeTargets.All)]
    public class CharListAttribute : Attribute
    {
        public int[] Characters { get; internal set; }

        public CharListAttribute(params int[] characters)
        {
            Characters = characters;
        }
    }
}
