namespace grbl.Master.Service.Interface
{
    using grbl.Master.Service.DataTypes;

    public interface IGrblCommandPreProcessor
    {
        void Process( ref Command cmd);
    }
}
