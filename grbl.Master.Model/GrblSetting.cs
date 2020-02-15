namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;

    using PropertyChanged;

    public class GrblSetting : NotifyPropertyChanged
    {
        public int Index { get; set; }

        public string Value { get; set; }
        public string OriginalValue { get; set; }

        [DependsOn(nameof(Value), nameof(OriginalValue))]
        public bool IsModified => Value != OriginalValue;

        public string Description => Resources.Mssages.ResourceManager.GetString($"Setting:{Index}:Description");

        public GrblSettingType Type { get; set; }
    }
}
