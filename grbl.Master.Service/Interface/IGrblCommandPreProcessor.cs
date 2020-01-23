namespace grbl.Master.Service.Interface
{
    using grbl.Master.Model;

    public interface IGrblCommandPreProcessor
    {
        void Process( ref Command cmd);
    }
}
