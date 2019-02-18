namespace grbl.Master.BL.Implementation
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.Annotations;
    using grbl.Master.Service.Interface;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class GrblStatusProcessor : IGrblStatusProcessor, INotifyPropertyChanged
    {
        private const string ResponseTag = "^<(.*?)>$";
        private const string MPosTag = "^MPos:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$";
        private const string WPosTag = "^WPos:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$";
        private const string WcoTag = "^WCO:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$";
        private const string BfTag = "^Bf:\\d+,\\d+$";
        private const string LnTag = "^Ln:\\d+$";
        private const string FTag = "^F:\\d+$";
        private const string FsTag = "^FS:\\d+,\\d+$";
        private const string PnTag = "^Pn:[A-Z]*$";
        private const string OvTag = "^Ov:\\d+(,\\d+){2}$";
        private const string ATag = "^A:[A-Z]*$";

        readonly Regex _responseReg = new Regex(ResponseTag);

        readonly Regex _mPosReg = new Regex(MPosTag);

        readonly Regex _wPosReg = new Regex(WPosTag);

        readonly Regex _wCoReg = new Regex(WcoTag);

        readonly Regex _bfReg = new Regex(BfTag);

        readonly Regex _lnReg = new Regex(LnTag);

        readonly Regex _fReg = new Regex(FTag);

        readonly Regex _fsReg = new Regex(FsTag);

        readonly Regex _pnReg = new Regex(PnTag);

        readonly Regex _ovReg = new Regex(OvTag);

        readonly Regex _aReg = new Regex(ATag);

        private readonly string _decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        //private IGrblStatusRequester _grblStatusRequester;
        private readonly IComService _comService;

        public GrblStatusProcessor(/*IGrblStatusRequester grblStatusRequester,*/ ICommandSender commandSender, IComService comService)
        {
            _comService = comService;
            //_grblStatusRequester = grblStatusRequester;

            commandSender.CommandFinished += CommandSenderCommandFinished;
            _comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
        }

        private void ComServiceConnectionStateChanged(object sender, EventArgs e)
        {
            GrblStatus.MachineState = _comService.IsConnected ? MachineState.Online : MachineState.Offline;
        }

        private void CommandSenderCommandFinished(object sender, Service.DataTypes.Command e)
        {
            if (!string.IsNullOrEmpty(e.Result) && _responseReg.IsMatch(e.Result))
            {
                var parts = e.Result.Split(new[] { '<', '>', '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Any())
                {
                    GrblStatus.MachineState = (MachineState)Enum.Parse(typeof(MachineState), parts.First());
                }

                foreach (var part in parts)
                {
                    if (_mPosReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                        {
                            GrblStatus.MachinePosition.Update(position);
                            GrblStatus.WorkPosition.Update(GrblStatus.MachinePosition - GrblStatus.WorkOffset);
                        }
                    }
                    else if (_wPosReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                        {
                            GrblStatus.WorkPosition.Update(position);
                            GrblStatus.MachinePosition.Update(GrblStatus.WorkPosition + GrblStatus.WorkOffset);
                        }
                    }
                    else if (_wCoReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                        {
                            GrblStatus.WorkOffset.Update(position);
                        }
                    }
                    else if (_bfReg.IsMatch(part))
                    {
                        var bufferParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (bufferParts.Length == 3)
                        {
                            if (int.TryParse(bufferParts[1], out var blocks)
                                && int.TryParse(bufferParts[2], out var bytes))
                            {
                                GrblStatus.BufferState.AvailableBlocks = blocks;
                                GrblStatus.BufferState.AvailableBytes = bytes;
                            }
                        }
                    }
                    else if (_lnReg.IsMatch(part))
                    {
                        var lineParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (lineParts.Length == 2)
                        {
                            if (int.TryParse(lineParts[1], out var line))
                            {
                                GrblStatus.LineNumber = line;
                            }
                        }
                    }
                    else if (_fReg.IsMatch(part))
                    {
                        var feedParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (feedParts.Length == 2)
                        {
                            if (int.TryParse(feedParts[1], out var feed))
                            {
                                GrblStatus.FeedAndSpeed.Feed = feed;
                            }
                        }
                    }
                    else if (_fsReg.IsMatch(part))
                    {
                        var feedSpeedParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (feedSpeedParts.Length == 3)
                        {
                            if (int.TryParse(feedSpeedParts[1], out var feed) && int.TryParse(
                                    feedSpeedParts[2],
                                    out var speed))
                            {
                                GrblStatus.FeedAndSpeed.Feed = feed;
                                GrblStatus.FeedAndSpeed.Speed = speed;
                            }
                        }
                    }
                    else if (_pnReg.IsMatch(part))
                    {
                        var pinParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pinParts.Length == 2)
                        {
                            GrblStatus.InputPinState.XLimitPin = pinParts[1].Contains('X');
                            GrblStatus.InputPinState.YLimitPin = pinParts[1].Contains('Y');
                            GrblStatus.InputPinState.ZLimitPin = pinParts[1].Contains('Z');
                            GrblStatus.InputPinState.ProbePin = pinParts[1].Contains('P');
                            GrblStatus.InputPinState.DoorPin = pinParts[1].Contains('D');
                            GrblStatus.InputPinState.HoldPin = pinParts[1].Contains('H');
                            GrblStatus.InputPinState.SoftResetPin = pinParts[1].Contains('R');
                            GrblStatus.InputPinState.CycleStartPin = pinParts[1].Contains('S');
                        }
                    }
                    else if (_ovReg.IsMatch(part))
                    {
                        var overrideParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (overrideParts.Length == 4)
                        {
                            if (int.TryParse(overrideParts[1], out var feed)
                                && int.TryParse(overrideParts[2], out var rapid) && int.TryParse(
                                    overrideParts[3],
                                    out var spindle))
                            {
                                GrblStatus.OverrideValues.Feed = feed;
                                GrblStatus.OverrideValues.Rapid = rapid;
                                GrblStatus.OverrideValues.Spindle = spindle;
                            }
                        }
                    }
                    else if (_aReg.IsMatch(part))
                    {
                        var accessoryParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (accessoryParts.Length == 2)
                        {
                            GrblStatus.AccessoryState.SpindleCw = accessoryParts[1].Contains('S');
                            GrblStatus.AccessoryState.SpindleCcw = accessoryParts[1].Contains('C');
                            GrblStatus.AccessoryState.Flood = accessoryParts[1].Contains('F');
                            GrblStatus.AccessoryState.Mist = accessoryParts[1].Contains('M');
                        }
                    }
                }
            }
        }

        private bool ParsePosition(string data, out Position result)
        {
            result = new Position();
            var posParts = data.Split(new[] { '[', ']', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (posParts.Length == 4)
            {
                if (decimal.TryParse(posParts[1].Replace(".", _decimalSeparator), out var xPos)
                    && decimal.TryParse(posParts[2].Replace(".", _decimalSeparator), out var yPos)
                    && decimal.TryParse(posParts[3].Replace(".", _decimalSeparator), out var zPos))
                {
                    result.X = xPos;
                    result.Y = yPos;
                    result.Z = zPos;
                    return true;
                }
            }

            return false;
        }

        public GrblStatus GrblStatus { get; } = new GrblStatus();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
