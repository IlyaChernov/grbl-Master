namespace grbl.Master.Model
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using grbl.Master.Model.Enum;

    using JetBrains.Annotations;

    public class GrblSetting : INotifyPropertyChanged
    {
        public int Index { get; set; }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsModified));
            }
        }

        public string OriginalValue { get; set; }

        public bool IsModified => Value != OriginalValue;

        public string Description => Resources.Mssages.ResourceManager.GetString($"Setting:{Index}:Description");

        public GrblSettingType Type { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
