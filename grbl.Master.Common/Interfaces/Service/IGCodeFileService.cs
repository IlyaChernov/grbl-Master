namespace grbl.Master.Common.Interfaces.Service
{
    using grbl.Master.Model;

    public interface IGCodeFileService
    {
        GCodeFile File { get; }

        void Load(string path);
    }
}
