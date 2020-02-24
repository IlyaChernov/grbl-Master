namespace grbl.Master.Service.Implementation
{
    using System.Collections.ObjectModel;

    using grbl.Master.Model;
    using grbl.Master.Service.Interface;
    using grbl.Master.Utilities;

    public class MacroService : IMacroService
    {
        public ObservableCollection<Macros> Macroses { get; internal set; }

        public void SaveMacroses()
        {
            var index = 0;
            foreach (var macros in Macroses)
            {
                macros.Index = index++;
            }

            Model.Properties.Settings.Default.MacrosesXML = Macroses.SerializeToXML();
            Model.Properties.Settings.Default.Save();
            Model.Properties.Settings.Default.Reload();
        }

        public void DeleteMacros(Macros mcrs)
        {
            Macroses.Remove(mcrs);
            SaveMacroses();
        }

        public void LoadMacroses()
        {
            if (!string.IsNullOrEmpty(Model.Properties.Settings.Default.MacrosesXML))
            {
                Macroses = Model.Properties.Settings.Default.MacrosesXML.XmlDeserialize<ObservableCollection<Macros>>();
            }
        }
    }
}
