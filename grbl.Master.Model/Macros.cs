namespace grbl.Master.Model
{
    using System;
    using System.Windows.Media;

    [Serializable]
    public class Macros
    {
        public string Name { get; set; }

        public string Command { get; set; }

        public int Index { get; set; } = -1;

        public Color? Color { get; set; } = null;
    }
}
