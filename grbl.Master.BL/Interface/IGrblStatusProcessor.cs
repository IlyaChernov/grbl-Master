namespace grbl.Master.BL.Interface
{
    using grbl.Master.Model;

    public  interface IGrblStatusProcessor
    {
        GrblStatus GrblStatus
        {
            get;
        }
    }
}
