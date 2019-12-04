namespace grbl.Master.Model.Enum
{
    using System.ComponentModel;

    public enum MotionMode
    {
        [Description("Rapid positioning")]
        G0,
        [Description("Linear interpolation")]
        G1,
        [Description("Circular interpolation, CW")]
        G2,
        [Description("Circular interpolation, CCW")]
        G3,
        [Description("Probe toward workpiece, with error")]
        G38_2,
        [Description("Probe toward workpiese")]
        G38_3,
        [Description("Probe away from workpiece, with error")]
        G38_4,
        [Description("Probe away from workpiece")]
        G38_5,
        [Description("Motion mode cancel, Canned cycle")]
        G80
    }
}