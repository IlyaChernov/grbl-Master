// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    using grbl.Master.Model.Attribute;

    public enum CommandType
    {
        [Description("$")]
        [Regex("^\\$$")]
        [ExpectedResponse(ResponseType.HelpMessage, ResponseType.Ok)]
        [RequestType(RequestType.System)]
        Help,
        [Description("$$")]
        [Regex("^\\${2}$")]
        [ExpectedResponse(ResponseType.SettingPrintout, ResponseType.Ok)]
        [RequestType(RequestType.System)]
        VIewSettings,
        [Description("$x=val")]
        [Regex("^\\$\\d+=.+$")]
        [ExpectedResponse(ResponseType.Ok)]
        [RequestType(RequestType.System)]
        SaveSetting,
        [Description("$#")]
        [Regex("^\\$#$")]
        [ExpectedResponse(ResponseType.ParameterPrintout, ResponseType.Ok)]
        [RequestType(RequestType.System)]
        ViewGCodeParams,
        [Description("$G")]
        [Regex("^\\$G$")]
        [ExpectedResponse(ResponseType.GCodeState, ResponseType.Ok)]
        [RequestType(RequestType.System)]
        ViewGCodeParser,
        [Description("$I")]
        [Regex("^\\$I$")]
        [ExpectedResponse(ResponseType.BuildInfo, ResponseType.Ok)]
        [RequestType(RequestType.System)]
        ViewBuildInfo,
        [Description("$N")]
        [Regex("^\\$N$")]
        [ExpectedResponse(ResponseType.StartupLinePrintout, ResponseType.Ok)]
        [RequestType(RequestType.System)]
        ViewStartUpBlock,
        [Description("$Nx=line")]
        [Regex("^\\$Nx=.*$")]
        [ExpectedResponse(ResponseType.Ok)]
        [RequestType(RequestType.System)]
        SaveStartupBlock,
        [Description("$C")]
        [Regex("^\\$C$")]
        [ExpectedResponse(ResponseType.Ok)]
        [RequestType(RequestType.System)]
        CheckGCodeMode,
        [Description("$X")]
        [Regex("^\\$X$")]
        [ExpectedResponse(ResponseType.Ok)]
        [RequestType(RequestType.System)]
        KillAlarm,
        [Description("$H")]
        [Regex("^\\$H$")]
        [ExpectedResponse(ResponseType.Ok)]
        [RequestType(RequestType.System)]
        RunHoming,
        [Description("$J=line")]
        [Regex("^\\$J=.*$")]
        [ExpectedResponse(ResponseType.Ok)]
        [RequestType(RequestType.System)]
        RunJogging,
        [Description("$RST=$/#/*")]
        [Regex("^\\$RST=.*$")]
        [ExpectedResponse(ResponseType.Ok)]
        [RequestType(RequestType.System)]
        RestoreEEPROM,
        [Description("$SLP")]
        [Regex("^\\$SLP$")]
        [ExpectedResponse(ResponseType.Ok)]
        [RequestType(RequestType.System)]
        EnableSleep,


        //Realtime
        [Description("0x18")]
        [CharList(0x18)]
        [RequestType(RequestType.Realtime)]
        SoftReset,
        [Description("?")]
        [CharList(0x3F)]
        [RequestType(RequestType.Realtime)]
        ViewStatusReport,
        [Description("~")]
        [CharList(0x7E)]
        [RequestType(RequestType.Realtime)]
        CycleStartResume,
        [Description("!")]
        [CharList(0x21)]
        [RequestType(RequestType.Realtime)]
        FeedHold,

        //Unknown result
        [Description("0x84")]
        [CharList(0x84)]
        [RequestType(RequestType.Realtime)]
        SafetyDoor,
        [Description("0x85")]
        [CharList(0x85)]
        [RequestType(RequestType.Realtime)]
        JogCancel,
        [Description("0x90-0x9D")]
        [CharList(0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D)]
        [RequestType(RequestType.Realtime)]
        FeedOverrides,
        [Description("0x9E")]
        [CharList(0x9E)]
        [RequestType(RequestType.Realtime)]
        ToggleSpindleStop,
        [Description("0xA0")]
        [CharList(0xA0)]
        [RequestType(RequestType.Realtime)]
        ToggleFlood,
        [Description("0XA1")]
        [CharList(0xA1)]
        [RequestType(RequestType.Realtime)]
        ToggleMist
    }
}
