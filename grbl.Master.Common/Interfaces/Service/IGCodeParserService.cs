namespace grbl.Master.Common.Interfaces.Service
{
    using grbl.Master.Model.GCode;

    public interface IGCodeParserService
    {
        bool GCodeLineToFrame(string frame, GCodeFrame lastCodeFrame, out GCodeFrame newCodeFrame);
    }
}
