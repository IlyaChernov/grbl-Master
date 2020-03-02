namespace grbl.Master.Service.Interface
{
    using grbl.Master.Model;

    public interface IApplicationSettingsService
    {
        ApplicationSettings Settings { get; }

        void Upgrade();
        void Save();

        //void DeleteMacros(Macros mcrs);

        void Load();
    }
}
