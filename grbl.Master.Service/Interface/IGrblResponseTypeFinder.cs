namespace grbl.Master.Service.Interface
{
    using grbl.Master.Service.Enum;

    public interface IGrblResponseTypeFinder
    {
        ResponseType GetType(string line);
    }
}
