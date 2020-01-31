namespace grbl.Master.Service.Interface
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using grbl.Master.Model;

    public interface IMacroService
    {
        ObservableCollection<Macros> Macroses { get; }

        void SaveMacroses();

        void DeleteMacros(Macros mcrs);

        void LoadMacroses();
    }
}
