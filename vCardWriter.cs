using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace InternetStandards.Ietf.vCard
{
    public class vCardWriter : IDisposable
    {
        private readonly StringWriter _contentLineWriter = new StringWriter();
        private readonly TextWriter _writer;

        public vCardWriter(Stream stream)
        {
            _writer = new StreamWriter(stream, Encoding.UTF8);
        }

        public void WriteStartContentLine(string group, string name)
        {
            if (group != null)
            {
                _contentLineWriter.Write(group);
                _contentLineWriter.Write('.');
            }

            _contentLineWriter.Write(name);
        }

        public void WriteStartContentLine(string name)
        {
            WriteStartContentLine(null, name);
        }

        #region "Write Property Values"

        public void WriteRawValue(string value)
        {
            _contentLineWriter.Write(':');
            _contentLineWriter.WriteLine(value);
        }

        public void WriteTextValue(string value)
        {
            WriteRawValue(value);
        }

        public void WriteTextListValue(string value, params string[] values)
        {
            WriteRawValue(value);
        }

        public void WriteUriValue(Uri value)
        {
            WriteRawValue(value.ToString());
        }

        public void WriteTimestampValue(DateTimeOffset value)
        {
        }

        public void WriteTimestampValue(DateTime value)
        {
            WriteRawValue(value.ToString("yyyyMMddThhmmss"));
        }

        public void WriteBooleanValue(bool value)
        {
            WriteRawValue(value ? "TRUE" : "FALSE");
        }

        public void WriteIntegerValue(long value, params long[] values)
        {
            var integerValue = new StringBuilder(value.ToString(CultureInfo.InvariantCulture));
            if(values != null)
                foreach (var integer in values)
                {
                    integerValue.Append(',');
                    integerValue.Append(integer.ToString(CultureInfo.InvariantCulture));
                }
            
            WriteRawValue(integerValue.ToString());
        }

        public void WriteFloatValue(double value, params double[] values)
        {
            var floatValue = new StringBuilder(value.ToString(CultureInfo.InvariantCulture));
            if(values != null)
                foreach (var @float in values)
                {
                    floatValue.Append(',');
                    floatValue.Append(@float.ToString(CultureInfo.InvariantCulture));
                }
            
            WriteRawValue(floatValue.ToString());
        }

        public void WriteUtcOffsetValue(TimeSpan value)
        {
            var sign = value.Ticks >= 0 ? "+" : string.Empty;
            WriteRawValue(sign + value.ToString("hhmm"));
        }

        /* public void WriteLanguageTagValue(LanguageTag languageTag)
        {
            WriteRawValue(languageTag.ToString());
        } */

        #endregion


        public void WriteValueParameter(ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.Text:
                    WriteAnyParameter("VALUE", "text");
                    break;
                case ValueType.Uri:
                    WriteAnyParameter("VALUE", "uri");
                    break;
                case ValueType.Date:
                    WriteAnyParameter("VALUE", "date");
                    break;
                case ValueType.Time:
                    WriteAnyParameter("VALUE", "time");
                    break;
                case ValueType.DateTime:
                    WriteAnyParameter("VALUE", "date-time");
                    break;
                case ValueType.DateAndOrTime:
                    WriteAnyParameter("VALUE", "date-and-or-time");
                    break;
                case ValueType.Timestamp:
                    WriteAnyParameter("VALUE", "timestamp");
                    break;
                case ValueType.Boolean:
                    WriteAnyParameter("VALUE", "boolean");
                    break;
                case ValueType.Integer:
                    WriteAnyParameter("VALUE", "integer");
                    break;
                case ValueType.Float:
                    WriteAnyParameter("VALUE", "float");
                    break;
                case ValueType.UtcOffset:
                    WriteAnyParameter("VALUE", "utc-offset");
                    break;
                case ValueType.LanguageTag:
                    WriteAnyParameter("VALUE", "language-tag");
                    break;
                case ValueType.IanaToken:
                    WriteAnyParameter("VALUE", "iana-token");
                    break;
                case ValueType.XName:
                    WriteAnyParameter("VALUE", "x-name");
                    break;
            }
        }

        public void WritePreferenceParameter(int value)
        {
            WriteAnyParameter("PREF", value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteAlternateIdParameter(string value)
        {
            WriteAnyParameter("ALTID", value);
        }

        public void WriteSortAsParameter(string value, params string[] values)
        {
            WriteAnyParameter("SORT-AS", value, values);
        }

        public
            void WriteAnyParameter(string ianaTokenOrXName, string value, params string[] values)
        {
            _contentLineWriter.Write(ianaTokenOrXName);
            _contentLineWriter.Write('=');
            _contentLineWriter.Write(value);
            if (values == null || values.Length <= 0) return;
            _contentLineWriter.Write(',');
            _contentLineWriter.Write(string.Join(",", values));
        }

        public void WriteEndContentLine()
        {
            var contentLine = _contentLineWriter.ToString();
            var physicalLineOctets = 0;

            var e = StringInfo.GetTextElementEnumerator(contentLine);
            while (e.MoveNext())
            {
                var characterBytesCount = Encoding.UTF8.GetByteCount((string)e.Current);
                if (physicalLineOctets + characterBytesCount > 75)
                {
                    _writer.WriteLine();
                    _writer.Write(" ");
                    physicalLineOctets = 1;
                }

                _writer.Write(e.Current);
                physicalLineOctets += characterBytesCount;
            }

            _writer.WriteLine();
            _writer.Flush();
        }

        public void Close()
        {
            _writer.Close();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }

    public enum ValueType
    {
        Text,
        Uri,
        Date,
        Time,
        DateTime,
        DateAndOrTime,
        Timestamp,
        Boolean,
        Integer,
        Float,
        UtcOffset,
        LanguageTag,
        IanaToken,
        XName
    }
}
