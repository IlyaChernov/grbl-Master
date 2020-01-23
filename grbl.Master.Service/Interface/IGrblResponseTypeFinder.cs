namespace grbl.Master.Service.Interface
{
    using grbl.Master.Model.Enum;

    public interface IGrblResponseTypeFinder
    {
        ResponseType GetType(string line);
    }
}
