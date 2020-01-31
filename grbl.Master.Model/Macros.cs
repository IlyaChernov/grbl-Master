namespace grbl.Master.Model
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class Macros
    {
        public string Name { get; set; }

        public string Command { get; set; }

        public int Index { get; set; } = -1;
    }
}
