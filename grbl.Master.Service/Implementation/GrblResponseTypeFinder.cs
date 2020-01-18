namespace grbl.Master.Service.Implementation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;

    public class GrblResponseTypeFinder : IGrblResponseTypeFinder
    {
        private const string OkTag = "^ok$";
        private const string ErrorTag = "^error:(\\d+?)$";
        private const string StatusReportTag = "^<(.*?)>$";
        private const string WelcomeTag = "^Grbl .{4} \\['\\$' for help\\]$";
        private const string AlarmTag = "^ALARM:(\\d+?)$";
        private const string SettingTag = "^\\$\\d{1,3}=.+?$";
        private const string FeedbackTag = "^\\[MSG:.*?\\]$";
        private const string GCodeStateTag = "^\\[GC:.*?\\]$";
        private const string HelpTag = "^\\[HLP:.*?\\]$";
        private const string ParameterTag = "^\\[(G54|G55|G56|G57|G58|G59|G28|G30|G92|TLO|PRB):.*?\\]$";
        private const string BuildInfoTag = "^\\[(VER|OPT):.*?\\]$";
        private const string EchoTag = "^\\[echo:.*?\\]$";
        private const string StartupLinePrintoutTag = "\\$N\\d+=.*";
        private const string StartupLineTag = " ^>((.+?:((ok)|(error:.*?)))|(:error:7))$";

        private readonly Dictionary<ResponseType, Regex> _regexes = new Dictionary<ResponseType, Regex>
                                                              {
                                                                  { ResponseType.Ok, new Regex(OkTag) },
                                                                  { ResponseType.Error, new Regex(ErrorTag) },
                                                                  { ResponseType.StatusReport, new Regex(StatusReportTag) },
                                                                  { ResponseType.WelcomeMessage, new Regex(WelcomeTag) },
                                                                  { ResponseType.Alarm, new Regex(AlarmTag) },
                                                                  { ResponseType.SettingPrintout, new Regex(SettingTag) },
                                                                  { ResponseType.FeedbackMessage, new Regex(FeedbackTag) },
                                                                  { ResponseType.GCodeState, new Regex(GCodeStateTag) },
                                                                  { ResponseType.HelpMessage, new Regex(HelpTag) },
                                                                  { ResponseType.ParameterPrintout, new Regex(ParameterTag) },
                                                                  { ResponseType.BuildInfo, new Regex(BuildInfoTag) },
                                                                  { ResponseType.Echo, new Regex(EchoTag) },
                                                                  { ResponseType.StartupLinePrintout, new Regex(StartupLinePrintoutTag) },
                                                                  { ResponseType.StartupLineExecution, new Regex(StartupLineTag) }
                                                              };

        public ResponseType GetType(string line)
        {
            var result = _regexes.Single(x => x.Value.IsMatch(line));
            return result.Key;
        }
    }
}