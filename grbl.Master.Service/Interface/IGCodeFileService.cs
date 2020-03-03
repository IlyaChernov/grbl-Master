namespace grbl.Master.Service.Interface
{
    using grbl.Master.Model;

    public interface IGCodeFileService
    {
        GCodeFile File { get; }

        void Load(string path);
    }
}
