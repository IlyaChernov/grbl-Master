namespace grbl.Master.Model.Attribute
{
    using System;

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
