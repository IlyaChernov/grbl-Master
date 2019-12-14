﻿namespace grbl.Master.Service.Implementation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using grbl.Master.Service.Attribute;
    using grbl.Master.Service.DataTypes;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;

    public class GrblCommandPreProcessor : IGrblCommandPreProcessor
    {
        private struct Metadata
        {
            public RequestType? Type;

            public string RegularExpression;

            private Regex _regex;

            public Regex Regex
            {
                get
                {
                    return _regex ??= (string.IsNullOrEmpty(RegularExpression)
                                               ? null
                                               : new Regex(RegularExpression));
                }
            }

            public List<int> CharList;

            public List<ResponseType> ExpectedTypes;
        }

        private Dictionary<CommandType, Metadata> _regexeTable;

        public GrblCommandPreProcessor()
        {
            PrepareregexTable();
        }

        private void PrepareregexTable()
        {
            _regexeTable = System.Enum.GetValues(typeof(CommandType)).Cast<CommandType>().ToDictionary(
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
                                 ? _regexeTable.Where(x => x.Value.Regex != null)
                                     .Single(x => x.Value.Regex.IsMatch(line))
                                 : _regexeTable.Where(x => x.Value.CharList != null)
                                     .Single(x => x.Value.CharList.Any(y => y == line[0]));

                cmd.Type = result.Value.Type;
                cmd.ExpectedResponses = result.Value.ExpectedTypes;
            }
            catch
            {
                cmd.Type = RequestType.GCode;
                cmd.ExpectedResponses = new List<ResponseType> { ResponseType.Ok };
            }
        }
    }
}
