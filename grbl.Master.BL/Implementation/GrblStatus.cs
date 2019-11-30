namespace grbl.Master.BL.Implementation
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text.RegularExpressions;
    using System.Threading;

    using grbl.Master.Service.DataTypes;

    public class GrblStatus : IGrblStatus
    {
        private readonly ICommandSender _commandSender;

        private readonly string _decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        readonly Subject<Unit> _stopSubject = new Subject<Unit>();

        private struct ResponseProcessingDefinition
        {
            public Func<string, string[]> SplitAction;
            public List<ResponseProcessor> ProcessActions;
        }

        private struct ResponseProcessor
        {
            public string TagExpression;

            private Regex _regex;
            public Regex Regex { get { return _regex ??= new Regex(TagExpression); } }

            public Action<string> Action;
        }

        private Dictionary<ResponseType, ResponseProcessingDefinition> ProcessingTable;

        public GrblStatus(IComService comService, ICommandSender commandSender)
        {
            _commandSender = commandSender;
            comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.ResponseReceived += CommandSenderResponseReceived;

            ProcessingTable = new Dictionary<ResponseType, ResponseProcessingDefinition>
                                                                                  {
                                                                                      {ResponseType.StatusReport, new ResponseProcessingDefinition{
                                                                                              SplitAction   =s =>s.Split(new[]{'<', '>', '|'},StringSplitOptions.RemoveEmptyEntries),
                                                                                              ProcessActions = new List<ResponseProcessor>
                                                                                                                   {
                                                                                                  new ResponseProcessor{TagExpression = "Idle|Run|Hold|Jog|Alarm|Door|Check|Home|Sleep", Action =
                                                                                                                           s =>
                                                                                                                               {
                                                                                                                                   Enum.TryParse<MachineState>(s, true, out var result);
                                                                                                                                   GrblStatusModel.MachineState = result;
                                                                                                                               }},

                                                                                                                       new ResponseProcessor{ TagExpression = "^MPos:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                                                                                part =>
                                                                                                                                                    {
                                                                                                                                                        if (ParsePosition(part, out var position))
                                                                                                                                                        {
                                                                                                                                                            GrblStatusModel.MachinePosition.Update(position);
                                                                                                                                                            GrblStatusModel.WorkPosition.Update(
                                                                                                                                                                GrblStatusModel.MachinePosition - GrblStatusModel.WorkOffset);
                                                                                                                                                        }
                                                                                                                                                    } },
                                                                                                                       new ResponseProcessor{ TagExpression = "^WPos:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                                                                                part =>
                                                                                                                                                    {
                                                                                                                                                        if (ParsePosition(part, out var position))
                                                                                                                                                        {
                                                                                                                                                            GrblStatusModel.WorkPosition.Update(position);
                                                                                                                                                            GrblStatusModel.MachinePosition.Update(
                                                                                                                                                                GrblStatusModel.WorkPosition + GrblStatusModel.WorkOffset);
                                                                                                                                                        }
                                                                                                                                                    } },
                                                                                                                       new ResponseProcessor{ TagExpression = "^WCO:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                                                                                part =>
                                                                                                                                                    {
                                                                                                                                                        if (ParsePosition(part, out var position))
                                                                                                                                                            GrblStatusModel.WorkOffset.Update(position);
                                                                                                                                                    } },
                                                                                                                       new ResponseProcessor{ TagExpression =  "^Bf:\\d+,\\d+$", Action =
                                                                                                                                                part =>
                                                                                                                                                    {
                                                                                                                                                        var bufferParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                                                                                                                        if (bufferParts.Length == 3)
                                                                                                                                                            if (int.TryParse(bufferParts[1], out var blocks)
                                                                                                                                                                && int.TryParse(bufferParts[2], out var bytes))
                                                                                                                                                            {
                                                                                                                                                                GrblStatusModel.BufferState.AvailableBlocks = blocks;
                                                                                                                                                                GrblStatusModel.BufferState.AvailableBytes = bytes;
                                                                                                                                                            }
                                                                                                                                                    } },
                                                                                                                       new ResponseProcessor{ TagExpression = "^Ln:\\d+$", Action =
                                                                                                                                                part =>
                                                                                                                                                    {
                                                                                                                                                        var lineParts = part.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                                                                                                                        if (lineParts.Length == 2)
                                                                                                                                                            if (int.TryParse(lineParts[1], out var line))
                                                                                                                                                                GrblStatusModel.LineNumber = line;
                                                                                                                                                    } },
                                                                                                                       new ResponseProcessor{ TagExpression = "^F:\\d+$", Action =
                                                                                                                                                part =>
                                                                                                                                                    {
                                                                                                                                                        var feedParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                                                                                                                        if (feedParts.Length == 2)
                                                                                                                                                            if (int.TryParse(feedParts[1], out var feed))
                                                                                                                                                                GrblStatusModel.FeedAndSpeed.Feed = feed;
                                                                                                                                                    } },
                                                                                                                       new ResponseProcessor{ TagExpression = "^FS:\\d+,\\d+$", Action =
                                                                                                                                                                                 part =>
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
                                                                                                                                                                                     } },
                                                                                                                       new ResponseProcessor{ TagExpression = "^Pn:[A-Z]*$", Action =
                                                                                                                                                part =>
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
                                                                                                                                                    } },
                                                                                                                       new ResponseProcessor{ TagExpression =  "^Ov:\\d+(,\\d+){2}$", Action =
                                                                                                                                                part =>
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
                                                                                                                                                    } },
                                                                                                                       new ResponseProcessor{ TagExpression = "^A:[A-Z]*$", Action =
                                                                                                                                                part =>
                                                                                                                                                    {
                                                                                                                                                        var accessoryParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                                                                                                                        if (accessoryParts.Length == 2)
                                                                                                                                                        {
                                                                                                                                                            GrblStatusModel.AccessoryState.SpindleCw = accessoryParts[1].Contains('S');
                                                                                                                                                            GrblStatusModel.AccessoryState.SpindleCcw = accessoryParts[1].Contains('C');
                                                                                                                                                            GrblStatusModel.AccessoryState.Flood = accessoryParts[1].Contains('F');
                                                                                                                                                            GrblStatusModel.AccessoryState.Mist = accessoryParts[1].Contains('M');
                                                                                                                                                        }
                                                                                                                                                    } },
                                                                                                                   }
                                                                                    }  }
                                                                                  };
        }

        private void CommandSenderResponseReceived(object sender, Response e)
        {
            if (!ProcessingTable.ContainsKey(e.Type)) return;

            var processor = ProcessingTable[e.Type];

            if (!processor.Equals(default))
            {
                    var parts = processor.SplitAction?.Invoke(e.Data) ?? new[] { e.Data };

                    foreach (var part in parts)
                    {
                        processor.ProcessActions.SingleOrDefault(x => x.Regex.IsMatch(part)).Action?.Invoke(part);
                    }
            }
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

       //public event EventHandler StatusReceived;

        private void Request()
        {
            _commandSender.Send("?");
            _commandSender.Send("$G");
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

        //protected void OnStatusReceived()
        //{
        //   // StatusReceived?.Invoke(this, EventArgs.Empty);
        //}
    }
}