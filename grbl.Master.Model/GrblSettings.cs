namespace grbl.Master.Model
{
    using grbl.Master.Model.Enum;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    public class GrblSettings : NotifyPropertyChanged
    {

        private readonly SynchronizationContext _uiContext;

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
                                                                             GrblSettingType.Mask3,
                                                                             new[]
                                                                                 {
                                                                                     10
                                                                                 }
                                                                         },
                                                                         {
                                                                             GrblSettingType.Mask8,
                                                                             new[]
                                                                                 {
                                                                                     2, 3, 23
                                                                                 }
                                                                         }
                                                                     };

        public ObservableCollection<GrblSetting> SettingsList { get; } = new ObservableCollection<GrblSetting>();

        public GrblSettings()
        {
            _uiContext = SynchronizationContext.Current;
        }

        public void AddOrUpdate(GrblSetting setting)
        {
            _uiContext.Send(
                x =>
                    {
                        var index = SettingsList.IndexOf(SettingsList.FirstOrDefault(y => y.Index == setting.Index));

                        setting.Type = FindType(setting.Index);

                        if (index >= 0)
                        {
                            SettingsList.RemoveAt(index);
                            SettingsList.Insert(index, setting);
                        }
                        else
                        {
                            SettingsList.Add(setting);
                        }

                        OnPropertyChanged(nameof(SettingsList));
                    },
                null);
        }

        private GrblSettingType FindType(int index)
        {
            if (_typeTable.Any(x => x.Value.Any(y => y == index)))
            {
                return _typeTable.FirstOrDefault(x => x.Value.Any(y => y == index)).Key;
            }

            return GrblSettingType.Integer;
        }
    }
}
