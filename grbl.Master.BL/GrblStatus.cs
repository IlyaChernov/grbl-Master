﻿namespace grbl.Master.BL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text.RegularExpressions;
    using System.Threading;

    using grbl.Master.Common.Enum;
    using grbl.Master.Common.Interfaces.BL;
    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Model;
    using grbl.Master.Model.Enum;
    using grbl.Master.Model.Interface;

    public class GrblStatus : IGrblStatus
    {
        private readonly ICommandSender _commandSender;

        private readonly IApplicationSettingsService _applicationSettingsService;

        private readonly string _decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        private readonly Subject<Unit> _stopSubject = new Subject<Unit>();

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

        private readonly Dictionary<ResponseType, ResponseProcessingDefinition> _processingTable;

        private string TranslateMessageKey(string key)
        {
            return Model.Resources.Mssages.ResourceManager.GetString(key);
        }

        public GrblStatus(IComService comService, ICommandSender commandSender, IGrblStatusModel grblStatusModel, IApplicationSettingsService applicationSettingsService)
        {
            _commandSender = commandSender;
            _applicationSettingsService = applicationSettingsService;
            GrblStatusModel = grblStatusModel;
            comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
            _commandSender.ResponseReceived += CommandSenderResponseReceived;

            _processingTable = new Dictionary<ResponseType, ResponseProcessingDefinition>
                              {
                                  { ResponseType.Alarm, new ResponseProcessingDefinition
                                                            {
                                                                ProcessActions = new List<ResponseProcessor>
                                                                                     {
                                                                                         new ResponseProcessor
                                                                                             {
                                                                                                 TagExpression = "ALARM:.*",
                                                                                                 Action = s =>
                                                                                                     {
                                                                                                         var msg = TranslateMessageKey(s.Replace("ALARM","Alarm"));
                                                                                                         GrblStatusModel.LastMessage = msg;
                                                                                                         GrblStatusModel.MachineState = MachineState.Alarm;
                                                                                                     }
                                                                                             }
                                                                                     }
                                                            }},
                                  { ResponseType.SettingPrintout, new ResponseProcessingDefinition
                                                                      {
                                                                          ProcessActions = new List<ResponseProcessor>
                                                                                               {
                                                                                                   new ResponseProcessor
                                                                                                       {
                                                                                                           TagExpression = "^\\$\\d{1,3}=.+?$",
                                                                                                           Action = s =>
                                                                                                               {
                                                                                                                   var parts = s.Split(new []{ '$','=' }, StringSplitOptions.RemoveEmptyEntries);
                                                                                                                   if(parts.Length == 2 && int.TryParse(parts[0], out var index))
                                                                                                                   {
                                                                                                                       var setting = new GrblSetting
                                                                                                                                         {
                                                                                                                           Index = index,
                                                                                                                           Value = parts[1],
                                                                                                                           OriginalValue = parts[1]
                                                                                                                                         };

                                                                                                                       GrblStatusModel.Settings.AddOrUpdate(setting);
                                                                                                                   }
                                                                                                               }
                                                                                                       }
                                                                                               }
                                                                      } },
                                  { ResponseType.FeedbackMessage, new ResponseProcessingDefinition
                                                                      {
                                                                          SplitAction =s =>s.Split(new[]{'[', ']'},StringSplitOptions.RemoveEmptyEntries),
                                                                          ProcessActions = new List<ResponseProcessor>{new ResponseProcessor
                                                                                                                           {
                                                                              TagExpression = "MSG:.*",
                                                                              Action = s =>
                                                                                  {
                                                                                      GrblStatusModel.LastMessage =s.Replace("MSG:", string.Empty);
                                                                                  }
                                                                                                                           }}
                                                                      }},
                                  { ResponseType.HelpMessage, new ResponseProcessingDefinition
                                                                      {
                                                                          SplitAction =s =>s.Split(new[]{'[', ']'},StringSplitOptions.RemoveEmptyEntries),
                                                                          ProcessActions = new List<ResponseProcessor>{new ResponseProcessor
                                                                                                                           {
                                                                                                                               TagExpression = "HLP:.*",
                                                                                                                               Action = s =>
                                                                                                                                   {
                                                                                                                                       GrblStatusModel.LastMessage =s.Replace("HLP:", string.Empty);
                                                                                                                                   }
                                                                                                                           }}
                                                                      }},
                                  { ResponseType.Error, new ResponseProcessingDefinition
                                                                  {
                                                                      ProcessActions = new List<ResponseProcessor>{new ResponseProcessor
                                                                                                                       {
                                                                                                                           TagExpression = "error:.*",
                                                                                                                           Action = s =>
                                                                                                                               {
                                                                                                                                   var msg = TranslateMessageKey(s);
                                                                                                                                   GrblStatusModel.LastMessage = msg;
                                                                                                                               }
                                                                                                                       }}
                                                                  }},
                                  {ResponseType.StatusReport, new ResponseProcessingDefinition{
                                          SplitAction   =s =>s.Split(new[]{'<', '>', '|'},StringSplitOptions.RemoveEmptyEntries),
                                          ProcessActions = new List<ResponseProcessor>
                                                               {
                                                                   new ResponseProcessor{TagExpression = "Alarm|Hold|Door", Action =
                                                                                           s =>
                                                                                               {
                                                                                                   if(s.Contains(':'))
                                                                                                   {
                                                                                                       var msg = TranslateMessageKey(s);
                                                                                                       GrblStatusModel.LastMessage = msg;
                                                                                                   }
                                                                                                   var parts = s.Split(new []{':'}, StringSplitOptions.RemoveEmptyEntries);
                                                                                                   Enum.TryParse<MachineState>(parts.FirstOrDefault(), true, out var result);
                                                                                                   GrblStatusModel.MachineState = result;
                                                                                               }},
                                                                   new ResponseProcessor{TagExpression = "Idle|Run|Jog|Check|Home|Sleep", Action =
                                                                                            s =>
                                                                                                {
                                                                                                    var parts = s.Split(new []{':'}, StringSplitOptions.RemoveEmptyEntries);
                                                                                                    Enum.TryParse<MachineState>(parts.FirstOrDefault(), true, out var result);
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
                                                                                                            GrblStatusModel.FeedAndSpeed.FeedRate = feed;
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
                                                                                                            GrblStatusModel.FeedAndSpeed.FeedRate = feed;
                                                                                                            GrblStatusModel.FeedAndSpeed.SpindleSpeed = speed;
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
                                                                                                        GrblStatusModel.InputPinState.UpdateDateTime = DateTime.Now;
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
                                                                                                        if(accessoryParts[1].Contains('S'))
                                                                                                        {
                                                                                                         GrblStatusModel.AccessoryState.Spindle = SpindleState.M3;
                                                                                                        }
                                                                                                        else if(accessoryParts[1].Contains('C'))
                                                                                                        {
                                                                                                            GrblStatusModel.AccessoryState.Spindle = SpindleState.M4;
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            GrblStatusModel.AccessoryState.Spindle = SpindleState.M5;
                                                                                                        }

                                                                                                        GrblStatusModel.AccessoryState.Flood = accessoryParts[1].Contains('F');

                                                                                                        GrblStatusModel.AccessoryState.Mist = accessoryParts[1].Contains('M');
                                                                                                    }
                                                                                                } }
                                                               }
                                }
                                  },
                                  {ResponseType.ParameterPrintout, new ResponseProcessingDefinition{
                                          SplitAction   =s =>s.Split(new[]{'[', ']'},StringSplitOptions.RemoveEmptyEntries),
                                          ProcessActions = new List<ResponseProcessor>
                                                               {
                                                                   new ResponseProcessor{ TagExpression = "^TLO:\\d+.\\d+$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    var lineParts = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                                                                    if (lineParts.Length == 2)
                                                                                                        if (ParseSingleAxisPosition(lineParts[1], out var line))
                                                                                                            GrblStatusModel.ToolLengthOffset = line;
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^PRB:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}:\\d$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    var lineParts = part.Split(new[] { ':', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                                                                                                    if (ParsePosition(part.Remove(part.Length-2), out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.ProbePosition.Update(position);
                                                                                                    }

                                                                                                    if (lineParts.Length == 3 && int.TryParse(lineParts.Last().Trim(), out var probingResult))
                                                                                                    {
                                                                                                        GrblStatusModel.ProbeState = probingResult!= 0;
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G54:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G54Position.Update(position);
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G55:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G55Position.Update(position);
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G56:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G56Position.Update(position);
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G57:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G57Position.Update(position);
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G58:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G58Position.Update(position);
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G59:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G59Position.Update(position);
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G28:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G28Position.Update(position);
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G30:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G30Position.Update(position);
                                                                                                    }
                                                                                                } },
                                                                   new ResponseProcessor{ TagExpression = "^G92:(-?\\d+(\\.\\d{3})?)(,-?\\d+(\\.\\d{3})?){2}$", Action =
                                                                                            part =>
                                                                                                {
                                                                                                    if (ParsePosition(part, out var position))
                                                                                                    {
                                                                                                        GrblStatusModel.G92Position.Update(position);
                                                                                                    }
                                                                                                } }
                                                               }
                                }
                                  },
                                  {ResponseType.GCodeState, new ResponseProcessingDefinition{
                                           SplitAction = s => s.Split(new []{':', ' '}, StringSplitOptions.RemoveEmptyEntries),
                                           ProcessActions = new List<ResponseProcessor>
                                                                {
                                                                    new ResponseProcessor{TagExpression = "^G(0|1|2|38\\.(2|3|4|5)|3|80)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<MotionMode>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.MotionMode = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^G5(4|5|6|7|8|9)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<CoordinateSystem>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.CoordinateSystem = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^G1(7|8|9)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<ActivePlane>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.ActivePlane = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^G9(0|1)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<DistanceMode>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.DistanceMode = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^G91\\.1$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<ArcDistanceMode>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.ArcDistanceMode = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^G9(3|4)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<FeedRateMode>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.FeedRateMode = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^G2(0|1)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<UnitsMode>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.UnitsMode = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^G40$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<CutterRadiusCompensation>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.CutterRadiusCompensation = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^G4(3\\.1|9)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<ToolLengthMode>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.ToolLengthMode = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^M(0|1|2|30)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<ProgramMode>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.ProgramMode = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^M(3|4|5)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<SpindleState>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.SpindleState = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^M(7|8|9)$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     Enum.TryParse<CoolantState>(s.Replace('.', '_'), true, out var result);
                                                                                                     GrblStatusModel.CoolantState = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^T\\d+$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     long.TryParse(s.Remove(0), out var result);
                                                                                                     GrblStatusModel.FeedAndSpeed.ToolNumber = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^S\\d+$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     long.TryParse(s.Remove(0), out var result);
                                                                                                     GrblStatusModel.FeedAndSpeed.SpindleSpeed = result;
                                                                                                 }},
                                                                    new ResponseProcessor{TagExpression = "^F\\d+$", Action =
                                                                                             s =>
                                                                                                 {
                                                                                                     long.TryParse(s.Remove(0), out var result);
                                                                                                     GrblStatusModel.FeedAndSpeed.FeedRate = result;
                                                                                                 }}
                                                                }
                                       }
                                  }
                              };
        }

        private void CommandSenderResponseReceived(object sender, Response e)
        {
            if (!_processingTable.ContainsKey(e.Type)) return;

            var processor = _processingTable[e.Type];

            if (!processor.Equals(default))
            {
                var parts = processor.SplitAction?.Invoke(e.Data) ?? new[] { e.Data };

                foreach (var part in parts)
                {
                    processor.ProcessActions.SingleOrDefault(x => x.Regex.IsMatch(part)).Action?.Invoke(part);
                }
            }

            if (DateTime.Now - GrblStatusModel.InputPinState.UpdateDateTime > TimeSpan.FromSeconds(_applicationSettingsService.Settings.PinStateResetTimeout))
            {
                GrblStatusModel.InputPinState.Reset();
            }
        }

        public IGrblStatusModel GrblStatusModel { get; }

        private bool _isRunning;

        public void StartRequesting(TimeSpan positionsInterval, TimeSpan gStateInterval, TimeSpan offsetsInterval)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Observable.Timer(TimeSpan.Zero, positionsInterval).TakeUntil(_stopSubject).Subscribe(l => { RequestPositions(); });
                Observable.Timer(TimeSpan.Zero, gStateInterval).TakeUntil(_stopSubject).Subscribe(l => { RequestGStatus(); });
                Observable.Timer(TimeSpan.Zero, offsetsInterval).TakeUntil(_stopSubject).Subscribe(l => { RequestOffsets(); });
            }
        }

        public void InitialRequest()
        {
            _commandSender.Send("$$");
        }

        public void StopRequesting()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _stopSubject.OnNext(Unit.Default);
            }
        }

        private void RequestPositions()
        {
            _commandSender.Send("?");
        }
        private void RequestGStatus()
        {
            _commandSender.Send("$G");
        }
        private void RequestOffsets()
        {
            _commandSender.Send("$#");
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            GrblStatusModel.MachineState =
                e == ConnectionState.Online ? MachineState.Online : MachineState.Offline;
        }

        private bool ParseSingleAxisPosition(string data, out decimal result)
        {
            result = 0;
            if (data.Length > 0)
                if (decimal.TryParse(data.Replace(".", _decimalSeparator), out result))
                {
                    return true;
                }

            return false;
        }

        private bool ParsePosition(string data, out Position result)
        {
            result = new Position();
            var posParts = data.Split(new[] { '[', ']', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (posParts.Length == 4)
                if (ParseSingleAxisPosition(posParts[1], out var xPos) &&
                    ParseSingleAxisPosition(posParts[2], out var yPos) &&
                    ParseSingleAxisPosition(posParts[3], out var zPos))
                {
                    result.X = xPos;
                    result.Y = yPos;
                    result.Z = zPos;
                    return true;
                }

            return false;
        }
    }
}