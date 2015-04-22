using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InternetStandards.Ietf.vCard.v4
{
    public struct vCardText
    {
        private string _text;

        public vCardText(string text)
        {
            _text = text;
        }

        public string Value
        {
            get
            {
                return Regex.Replace(_text, @"\\n", "\u2028", RegexOptions.IgnoreCase);
            }
            set
            {
                _text = Regex.Replace(value, @"\\n", @"\n", RegexOptions.IgnoreCase);
            }
        }

        public override string ToString()
        {
            return _text;
        }
    }

    public struct vCardUri
    {
        private string _uri;

        public vCardUri(string uri)
        {
            _uri = new Uri(uri).ToString();
        }

        public Uri Value
        {
            get
            {
                return new Uri(_uri);
            }
            set
            {
                _uri = new Uri(_uri).ToString();
            }
        }

        public override string ToString()
        {
            return _uri;
        }
    }

    public struct vCardTimestamp
    {
        private string _timestamp;

        public vCardTimestamp(string timestamp)
        {
            _timestamp = timestamp;
        }

        public DateTimeOffset Value
        {
            get
            {
                return DateTimeOffset.Parse(_timestamp);
            }
            set
            {
                _timestamp = value.ToString();
            }
        }

        public override string ToString()
        {
            return _timestamp;
        }
    }
}
