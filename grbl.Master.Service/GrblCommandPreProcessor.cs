namespace grbl.Master.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Model;
    using grbl.Master.Model.Attribute;
    using grbl.Master.Model.Enum;
    using grbl.Master.Model.Interface;
    using grbl.Master.Service.Enum;

    using Jint;

    public class GrblCommandPreProcessor : IGrblCommandPreProcessor
    {
        private readonly IGrblStatusModel _grblStatusModel;

        private readonly Engine _evaluator;

        readonly Regex _macroTag = new Regex("{[^{}]+}");

        private struct Metadata
        {
            public RequestType? Type;

            public string RegularExpression;

            private Regex _regex;

            public Regex Regex
            {
                get
                {
                    return _regex ??= string.IsNullOrEmpty(RegularExpression)
                                          ? null
                                          : new Regex(RegularExpression);
                }
            }

            public List<int> CharList;

            public List<ResponseType> ExpectedTypes;
        }

        private Dictionary<CommandType, Metadata> _regexTable;

        public GrblCommandPreProcessor(IGrblStatusModel grblStatusModel)
        {
            _grblStatusModel = grblStatusModel;
            _evaluator = new Engine();

            PrepareRegexTable();
        }

        private void PrepareRegexTable()
        {
            _regexTable = System.Enum.GetValues(typeof(CommandType)).Cast<CommandType>().ToDictionary(
                type => type,
                type => new Metadata
                {
                    RegularExpression = type.GetAttributeOfType<RegexAttribute>()?.RegularExpression,
                    ExpectedTypes =
                                    type.GetAttributeOfType<ExpectedResponseAttribute>()?.ResponseTypes?.ToList(),
                    CharList = type.GetAttributeOfType<CharListAttribute>()?.Characters?.ToList(),
                    Type = type.GetAttributeOfType<RequestTypeAttribute>()?.Type
                });
        }

        public void Process(ref Command cmd)
        {
            var line = cmd.Data;
            try
            {
                var result = line.Length > 1
                                 ? _regexTable.Where(x => x.Value.Regex != null)
                                     .Single(x => x.Value.Regex.IsMatch(line))
                                 : _regexTable.Where(x => x.Value.CharList != null)
                                     .Single(x => x.Value.CharList.Any(y => y == line[0]));

                cmd.Type = result.Value.Type;
                cmd.ExpectedResponses = result.Value.ExpectedTypes;
            }
            catch
            {
                cmd.Type = RequestType.GCode;
                cmd.ExpectedResponses = new List<ResponseType> { ResponseType.Ok, ResponseType.Error };
            }

            if (cmd.Source == CommandSourceType.Macros)
            {
                cmd.Data = UnwrapMacros(line);
            }
        }

        private string UnwrapMacros(string line)
        {
            _evaluator.SetValue("STATUS", _grblStatusModel);

            var matches = _macroTag.Matches(line);

            foreach (Match match in matches)
            {
                var macroPart = match.Value.Replace("{", "").Replace("}", "");

                var result = Evaluate(macroPart);

                line = line.Replace(match.Value, result);
            }

            if (matches.Count > 0)
            {
                line = UnwrapMacros(line);
            }

            return line;
        }

        private string Evaluate(string line)
        {
            try
            {
                return (_evaluator.Execute(line.Replace(',', '.')).GetCompletionValue().ToObject() ?? string.Empty).ToString()
                    .Replace(',', '.');
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
