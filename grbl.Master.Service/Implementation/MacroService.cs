namespace grbl.Master.Service.Implementation
{
    using System.Collections.ObjectModel;

    using grbl.Master.Model;
    using grbl.Master.Service.Interface;
    using grbl.Master.Utilities;

    public class MacroService : IMacroService
    {
        public ObservableCollection<Macros> Macroses { get; internal set; }// = new ObservableCollection<Macros> { new Macros { Name = "Test1", Command = "M0" }, new Macros { Name = "Test2", Command = "M0" } };

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
            Macroses = Model.Properties.Settings.Default.MacrosesXML.XmlDeserialize<ObservableCollection<Macros>>();
        }
    }
}
