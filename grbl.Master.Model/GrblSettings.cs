namespace grbl.Master.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using grbl.Master.Model.Enum;

    using JetBrains.Annotations;

    public class GrblSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private readonly Dictionary<GrblSettingType, int[]> _typeTable = new Dictionary<GrblSettingType, int[]>
                                                                     {
                                                                         {
                                                                             GrblSettingType.Integer,
                                                                             new[]
                                                                                 {
                                                                                     0, 1, 26, 30, 31
                                                                                 }
                                                                         },
                                                                         {
                                                                             GrblSettingType.Boolean,
                                                                             new[]
                                                                                 {
                                                                                     4, 5, 6, 13, 20, 21, 22, 32
                                                                                 }
                                                                         },
                                                                         {
                                                                             GrblSettingType.Decimal,
                                                                             new[]
                                                                                 {
                                                                                     11, 12, 24, 25, 27, 100, 101, 102,
                                                                                     110, 111, 112, 120, 121, 122, 130,
                                                                                     131, 132
                                                                                 }
                                                                         },
                                                                         {
                                                                             GrblSettingType.Mask,
                                                                             new[]
                                                                                 {
                                                                                     2, 3, 10, 23
                                                                                 }
                                                                         }
                                                                     };

        public List<GrblSetting> Settings { get; } = new List<GrblSetting>();

        public void AddOrUpdate(GrblSetting setting)
        {
            Settings.Remove(Settings.FirstOrDefault(x => x.Index == setting.Index));
            setting.Type = FindType(setting.Index);
            Settings.Add(setting);
            OnPropertyChanged(nameof(Settings));
        }

        private GrblSettingType FindType(int index)
        {
            if (_typeTable.Any(x => x.Value.Any(y => y == index)))
            {
                return _typeTable.FirstOrDefault(x => x.Value.Any(y => y == index)).Key;
            }

            return GrblSettingType.Integer;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
