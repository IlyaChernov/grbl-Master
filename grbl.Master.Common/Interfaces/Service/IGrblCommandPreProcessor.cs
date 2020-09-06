namespace grbl.Master.Common.Interfaces.Service
{
    using grbl.Master.Model;

    public interface IGrblCommandPreProcessor
    {
        void Process( ref Command cmd);
    }
}
