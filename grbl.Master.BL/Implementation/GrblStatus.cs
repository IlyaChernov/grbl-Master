namespace grbl.Master.BL.Implementation
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class GrblStatus : IGrblStatus
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

        private readonly Regex _aReg = new Regex(ATag);

        private readonly Regex _bfReg = new Regex(BfTag);

        private readonly ICommandSender _commandSender;

        private readonly string _decimalSeparator =
            Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        private readonly Regex _fReg = new Regex(FTag);

        private readonly Regex _fsReg = new Regex(FsTag);

        private readonly Regex _lnReg = new Regex(LnTag);

        private readonly Regex _mPosReg = new Regex(MPosTag);

        private readonly Regex _ovReg = new Regex(OvTag);

        private readonly Regex _pnReg = new Regex(PnTag);

        private readonly Regex _responseReg = new Regex(ResponseTag);

        private readonly Regex _wCoReg = new Regex(WcoTag);

        private readonly Regex _wPosReg = new Regex(WPosTag);

        readonly Subject<Unit> _stopSubject = new Subject<Unit>();

        public GrblStatus(IComService comService, ICommandSender commandSender)
        {
            _commandSender = commandSender;
            comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            comService.LineReceived += ComServiceLineReceived;
        }

        public GrblStatusModel GrblStatusModel { get; set; } = new GrblStatusModel();

        public bool IsRunning
        {
            get; private set;
        }

        public void StartRequesting(TimeSpan interval)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                Observable.Timer(TimeSpan.Zero, interval).TakeUntil(_stopSubject).Subscribe(l => { Request(); });
            }
        }

        public void StopRequesting()
        {
            if (IsRunning)
            {
                IsRunning = false;
                _stopSubject.OnNext(Unit.Default);
            }
        }

        public event EventHandler StatusReceived;

        private void Request()
        {
            _commandSender.Send("?", CommandType.Realtime);
            _commandSender.Send("$G", CommandType.System);
        }

        private void ComServiceLineReceived(object sender, string e)
        {
            if (!string.IsNullOrEmpty(e) && _responseReg.IsMatch(e))
            {
                var parts = e.Split(new[] { '<', '>', '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Any())
                {
                    var firstPart = parts.First().Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).First();
                    GrblStatusModel.MachineState = (MachineState)Enum.Parse(typeof(MachineState), firstPart);
                }

                foreach (var part in parts)
                    if (_mPosReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                        {
                            GrblStatusModel.MachinePosition.Update(position);
                            GrblStatusModel.WorkPosition.Update(
                                GrblStatusModel.MachinePosition - GrblStatusModel.WorkOffset);
                        }
                    }
                    else if (_wPosReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                        {
                            GrblStatusModel.WorkPosition.Update(position);
                            GrblStatusModel.MachinePosition.Update(
                                GrblStatusModel.WorkPosition + GrblStatusModel.WorkOffset);
                        }
                    }
                    else if (_wCoReg.IsMatch(part))
                    {
                        if (ParsePosition(part, out var position))
                            GrblStatusModel.WorkOffset.Update(position);
                    }
                    else if (_bfReg.IsMatch(part))
                    {
                        var bufferParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (bufferParts.Length == 3)
                            if (int.TryParse(bufferParts[1], out var blocks)
                                && int.TryParse(bufferParts[2], out var bytes))
                            {
                                GrblStatusModel.BufferState.AvailableBlocks = blocks;
                                GrblStatusModel.BufferState.AvailableBytes = bytes;
                            }
                    }
                    else if (_lnReg.IsMatch(part))
                    {
                        var lineParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (lineParts.Length == 2)
                            if (int.TryParse(lineParts[1], out var line))
                                GrblStatusModel.LineNumber = line;
                    }
                    else if (_fReg.IsMatch(part))
                    {
                        var feedParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (feedParts.Length == 2)
                            if (int.TryParse(feedParts[1], out var feed))
                                GrblStatusModel.FeedAndSpeed.Feed = feed;
                    }
                    else if (_fsReg.IsMatch(part))
                    {
                        var feedSpeedParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (feedSpeedParts.Length == 3)
                            if (int.TryParse(feedSpeedParts[1], out var feed) && int.TryParse(
                                    feedSpeedParts[2],
                                    out var speed))
                            {
                                GrblStatusModel.FeedAndSpeed.Feed = feed;
                                GrblStatusModel.FeedAndSpeed.Speed = speed;
                            }
                    }
                    else if (_pnReg.IsMatch(part))
                    {
                        var pinParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pinParts.Length == 2)
                        {
                            GrblStatusModel.InputPinState.XLimitPin = pinParts[1].Contains('X');
                            GrblStatusModel.InputPinState.YLimitPin = pinParts[1].Contains('Y');
                            GrblStatusModel.InputPinState.ZLimitPin = pinParts[1].Contains('Z');
                            GrblStatusModel.InputPinState.ProbePin = pinParts[1].Contains('P');
                            GrblStatusModel.InputPinState.DoorPin = pinParts[1].Contains('D');
                            GrblStatusModel.InputPinState.HoldPin = pinParts[1].Contains('H');
                            GrblStatusModel.InputPinState.SoftResetPin = pinParts[1].Contains('R');
                            GrblStatusModel.InputPinState.CycleStartPin = pinParts[1].Contains('S');
                        }
                    }
                    else if (_ovReg.IsMatch(part))
                    {
                        var overrideParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (overrideParts.Length == 4)
                            if (int.TryParse(overrideParts[1], out var feed)
                                && int.TryParse(overrideParts[2], out var rapid) && int.TryParse(
                                    overrideParts[3],
                                    out var spindle))
                            {
                                GrblStatusModel.OverrideValues.Feed = feed;
                                GrblStatusModel.OverrideValues.Rapid = rapid;
                                GrblStatusModel.OverrideValues.Spindle = spindle;
                            }
                    }
                    else if (_aReg.IsMatch(part))
                    {
                        var accessoryParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (accessoryParts.Length == 2)
                        {
                            GrblStatusModel.AccessoryState.SpindleCw = accessoryParts[1].Contains('S');
                            GrblStatusModel.AccessoryState.SpindleCcw = accessoryParts[1].Contains('C');
                            GrblStatusModel.AccessoryState.Flood = accessoryParts[1].Contains('F');
                            GrblStatusModel.AccessoryState.Mist = accessoryParts[1].Contains('M');
                        }
                    }

                OnStatusReceived();
            }
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            GrblStatusModel.MachineState =
                e == ConnectionState.Online ? MachineState.Online : MachineState.Offline;
        }

        private bool ParsePosition(string data, out Position result)
        {
            result = new Position();
            var posParts = data.Split(new[] { '[', ']', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (posParts.Length == 4)
                if (decimal.TryParse(posParts[1].Replace(".", _decimalSeparator), out var xPos)
                    && decimal.TryParse(posParts[2].Replace(".", _decimalSeparator), out var yPos)
                    && decimal.TryParse(posParts[3].Replace(".", _decimalSeparator), out var zPos))
                {
                    result.X = xPos;
                    result.Y = yPos;
                    result.Z = zPos;
                    return true;
                }

            return false;
        }

        protected void OnStatusReceived()
        {
            StatusReceived?.Invoke(this, EventArgs.Empty);
        }
    }
}