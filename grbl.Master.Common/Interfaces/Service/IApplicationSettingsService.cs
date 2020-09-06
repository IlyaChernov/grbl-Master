namespace grbl.Master.Common.Interfaces.Service
{
    using grbl.Master.Model;

    public interface IApplicationSettingsService
    {
        ApplicationSettings Settings { get; }

        void Upgrade();
        void Save();
        void Load();
    }
}
