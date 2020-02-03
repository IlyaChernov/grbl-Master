namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;

    public class GrblSetting
    {
        public int Index { get; set; }

        public string Value { get; set; }

        public string Description => Resources.Mssages.ResourceManager.GetString($"Setting:{Index}:Description");

        public GrblSettingType Type { get; set; }
    }
}
