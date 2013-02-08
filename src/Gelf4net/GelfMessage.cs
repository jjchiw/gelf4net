using System;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace gelf4net
{
    public class GelfMessage : Dictionary<string, object>
    {
        public string Facility
        {
            get
            {
                if (!this.ContainsKey("facility"))
                    return null;

                return (string)this["facility"];
            }
            set
            {
                if (!this.ContainsKey("facility"))
                    this.Add("facility", value);
                else
                    this["facility"] = value;
            }
        }

        public string File
        {
            get
            {
                if (!this.ContainsKey("file"))
                    return null;

                return (string)this["file"];
            }
            set
            {
                if (!this.ContainsKey("file"))
                    this.Add("file", value);
                else
                    this["file"] = value;
            }
        }

        public string FullMessage
        {
            get 
            {
                if (!this.ContainsKey("full_message"))
                    return null;

                return (string)this["full_message"];
            }
            set
            {
                if (!this.ContainsKey("full_message"))
                    this.Add("full_message", value);
                else
                    this["full_message"] = value;
            }
        }

        public string Host
        {
            get
            {
                if (!this.ContainsKey("host"))
                    return null;

                return (string)this["host"];
            }
            set
            {
                if (!this.ContainsKey("host"))
                    this.Add("host", value);
                else
                    this["host"] = value;
            }
        }

        public long Level
        {
            get
            {
                if (!this.ContainsKey("level"))
                    return int.MinValue;

                return (long)this["level"];
            }
            set
            {
                if (!this.ContainsKey("level"))
                    this.Add("level", value);
                else
                    this["level"] = value;
            }
        }

        public string Line
        {
            get
            {
                if (!this.ContainsKey("line"))
                    return null;

                return (string)this["line"];
            }
            set
            {
                if (!this.ContainsKey("line"))
                    this.Add("line", value);
                else
                    this["line"] = value;
            }
        }

        public string ShortMessage
        {
            get
            {
                if (!this.ContainsKey("short_message"))
                    return null;

                return (string)this["short_message"];
            }
            set
            {
                if (!this.ContainsKey("short_message"))
                    this.Add("short_message", value);
                else
                    this["short_message"] = value;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                if (!this.ContainsKey("timestamp"))
                    return DateTime.MinValue;

                var val = this["timestamp"];
                double value;
                var parsed = double.TryParse(val as string, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                return parsed ? value.FromUnixTimestamp() : DateTime.MinValue;
            }
            set
            {
                if (!this.ContainsKey("timestamp"))
                    this.Add("timestamp", value.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture));
                else
                    this["timestamp"] = value.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture);
            }
        }

        public string Version
        {
            get
            {
                if (!this.ContainsKey("version"))
                    return null;

                return (string)this["version"];
            }
            set
            {
                if (!this.ContainsKey("version"))
                    this.Add("version", value);
                else
                    this["version"] = value;
            }
        }
    }
}