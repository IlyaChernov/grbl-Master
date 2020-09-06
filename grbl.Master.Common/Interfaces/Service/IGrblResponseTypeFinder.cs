namespace grbl.Master.Common.Interfaces.Service
{
    using grbl.Master.Model.Enum;

    public interface IGrblResponseTypeFinder
    {
        ResponseType GetType(string line);
    }
}
