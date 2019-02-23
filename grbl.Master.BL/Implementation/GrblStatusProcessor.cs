namespace grbl.Master.BL.Implementation
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class GrblStatusProcessor : IGrblStatusProcessor
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

        private readonly IGrblStatus _grblStatus;

        public GrblStatusProcessor(IComService comService, IGrblStatus grblStatus)
        {
            _grblStatus = grblStatus;

            comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            comService.LineReceived += ComServiceLineReceived;
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            if (!string.IsNullOrEmpty(e) && _responseReg.IsMatch(e))
            {
                var parts = e.Split(new[] { '<', '>', '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Any())
                {
                    _grblStatus.MachineState = (MachineState)Enum.Parse(typeof(MachineState), parts.First());
                }

                foreach (var part in parts)
                {
                    if (_mPosReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                        {
                            _grblStatus.MachinePosition.Update(position);
                            _grblStatus.WorkPosition.Update(_grblStatus.MachinePosition - _grblStatus.WorkOffset);
                        }
                    }
                    else if (_wPosReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                        {
                            _grblStatus.WorkPosition.Update(position);
                            _grblStatus.MachinePosition.Update(_grblStatus.WorkPosition + _grblStatus.WorkOffset);
                        }
                    }
                    else if (_wCoReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                        {
                            _grblStatus.WorkOffset.Update(position);
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
                                _grblStatus.BufferState.AvailableBlocks = blocks;
                                _grblStatus.BufferState.AvailableBytes = bytes;
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
                                _grblStatus.LineNumber = line;
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
                                _grblStatus.FeedAndSpeed.Feed = feed;
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
                                _grblStatus.FeedAndSpeed.Feed = feed;
                                _grblStatus.FeedAndSpeed.Speed = speed;
                            }
                        }
                    }
                    else if (_pnReg.IsMatch(part))
                    {
                        var pinParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pinParts.Length == 2)
                        {
                            _grblStatus.InputPinState.XLimitPin = pinParts[1].Contains('X');
                            _grblStatus.InputPinState.YLimitPin = pinParts[1].Contains('Y');
                            _grblStatus.InputPinState.ZLimitPin = pinParts[1].Contains('Z');
                            _grblStatus.InputPinState.ProbePin = pinParts[1].Contains('P');
                            _grblStatus.InputPinState.DoorPin = pinParts[1].Contains('D');
                            _grblStatus.InputPinState.HoldPin = pinParts[1].Contains('H');
                            _grblStatus.InputPinState.SoftResetPin = pinParts[1].Contains('R');
                            _grblStatus.InputPinState.CycleStartPin = pinParts[1].Contains('S');
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
                                _grblStatus.OverrideValues.Feed = feed;
                                _grblStatus.OverrideValues.Rapid = rapid;
                                _grblStatus.OverrideValues.Spindle = spindle;
                            }
                        }
                    }
                    else if (_aReg.IsMatch(part))
                    {
                        var accessoryParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (accessoryParts.Length == 2)
                        {
                            _grblStatus.AccessoryState.SpindleCw = accessoryParts[1].Contains('S');
                            _grblStatus.AccessoryState.SpindleCcw = accessoryParts[1].Contains('C');
                            _grblStatus.AccessoryState.Flood = accessoryParts[1].Contains('F');
                            _grblStatus.AccessoryState.Mist = accessoryParts[1].Contains('M');
                        }
                    }
                }

                OnStatusReceived();
            }
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            _grblStatus.MachineState = e == ConnectionState.Online ? MachineState.Online : MachineState.Offline;
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

        public event EventHandler StatusReceived;

        protected void OnStatusReceived()
        {
            StatusReceived?.Invoke(this, EventArgs.Empty);
        }
    }
}
