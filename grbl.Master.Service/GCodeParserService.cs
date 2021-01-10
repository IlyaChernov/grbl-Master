namespace grbl.Master.Service
{
    using System.Text.RegularExpressions;

    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Model.GCode;

    public class GCodeParserService : IGCodeParserService
    {
        //private Regex xRegex = new Regex(@"([Xx])((-?\d+)([,.]\d+)?|([,.]\d+))");
        //private Regex yRegex = new Regex(@"([Yy])((-?\d+)([,.]\d+)?|([,.]\d+))");
        //private Regex zRegex = new Regex(@"([Zz])((-?\d+)([,.]\d+)?|([,.]\d+))");

        public GCodeParserService()
        {
        }

        public bool GCodeLineToFrame(string frame, GCodeFrame lastCodeFrame, out GCodeFrame newCodeFrame)
        {
            newCodeFrame = new GCodeFrame();

            return false;
        }
    }
}
