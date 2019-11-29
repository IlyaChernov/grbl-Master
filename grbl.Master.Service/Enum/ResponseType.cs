namespace grbl.Master.Service.Enum
{
    public enum ResponseType
    {
        Ok,
        Error,
        StatusReport,
        WelcomeMessage,
        Alarm,
        SettingPrintout,
        FeedbackMessage,
        GCodeState,
        HelpMessage,
        ParameterPrintout,
        BuildInfo,
        Echo,
        StartupLinePrintout,
        StartupLineExecution
    }
}
