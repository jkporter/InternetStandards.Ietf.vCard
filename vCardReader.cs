using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InternetStandards.Ietf.vCard
{
    public class vCardReader
    {
        const string SafeCharPattern = @"(?:[\x20\t!\x23-\x39\x3C-\x7E]|[^\x00-\x7F])";
        const string QSafeCharPattern = @"(?:[\x20\t!\x23-\x7E]|[^\x00-\x7F])";
        
        const string GroupPattern = @"(?<group>[-A-Za-z0-9]+)";
        const string NamePattern = @"(?<name>[-A-Za-z0-9]+)";
        const string AnyParamPattern = @"(?<anyParam>[-A-Za-z0-9]+)";
        const string ParamValuePattern = @"(?<paramValue>" + SafeCharPattern + @"*|(?:""" + QSafeCharPattern + @"*""))";
        const string ParamPattern = @"(?<param>" + AnyParamPattern + @"=" + ParamValuePattern + @"(?:," + ParamValuePattern + @")*)";
        const string ValuePattern = @"(?<value>(?:[\x20\t\x21-\x7E]|[^\x00-\x7F])*)";
        const string ContentLinePattern =
            @"^(?:" + GroupPattern + @"\.)?" + NamePattern + @"(?:;" + ParamPattern + ")*:" + ValuePattern + "$";

        private static readonly Regex ContentLineRegEx = new Regex(ContentLinePattern);
        
        private readonly TextReader _reader;
        private string _line;

        public vCardReader(Stream input)
        {
            _reader = new StreamReader(input, Encoding.UTF8);
            if (_reader.ReadLine() != "BEGIN:VCARD" || _reader.ReadLine() != "VERSION:4.0")
                throw new Exception();
            _line = _reader.ReadLine();
        }

        public bool Read()
        {
            if (_line == "END:VCARD")
                return false;

            var contentLine = new StringBuilder(_line);
            bool folded;
            do
            {
                _line = _reader.ReadLine();
                folded = _line[0] == '\u0020' || _line[0] == '\u0009';
                if (folded)
                    contentLine.Append(_line.Substring(1));

            } while (folded);

            var m = ContentLineRegEx.Match(contentLine.ToString());
            if (!m.Success)
                throw new Exception();

            Group = m.Groups["group"].Success ? m.Groups["group"].Value : null;
            Name = m.Groups["name"].Value;


            Parameters = m.Groups["param"].Captures.Cast<Capture>().Select(c =>
            {
                var anyParam = m.Groups["anyParam"].Captures.Cast<Capture>().Single(c2 => c2.Index == c.Index).Value;
                var paramValues =
                    m.Groups["paramValue"].Captures.Cast<Capture>()
                        .Where(c2 => c2.Index > c.Index && c2.Index < c.Index + c.Length)
                        .Select(c2 => c2.Value);

                return new KeyValuePair<string, string[]>(anyParam, paramValues.ToArray());
            }).ToArray();

            Value = m.Groups["value"].Value;

            return true;
        }

        public string Group
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public KeyValuePair<string, string[]>[] Parameters
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }
    }
}
