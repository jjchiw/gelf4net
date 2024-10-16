using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gelf4Net.Layout
{
    public class GelfLayout : LayoutSkeleton
    {
        private const string GELF_VERSION = "1.0";
        private const int SHORT_MESSAGE_LENGTH = 250;

        private string _additionalFields;
        private PatternLayout _patternLayout;

        public GelfLayout()
        {
            Facility = Facility ?? "Gelf";
            IncludeLocationInformation = false;
            LogStackTraceFromMessage = true;
            IgnoresException = false;
            _patternLayout = new PatternLayout();
            SendTimeStampAsString = true;
        }

        /// <summary>
        /// The content type output by this layout.
        /// </summary>
        public override string ContentType
        {
            get
            {
                return "application/json";
            }
        }

        /// <summary>
        /// Gets or sets Facility.
        /// </summary>
        public string Facility { get; set; }

        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        public string HostName { get; set; }

        public string FieldSeparator { get; set; }

        public string KeyValueSeparator { get; set; }

        /// <summary>
        /// The additional fields configured in log4net config.
        /// </summary>
        public string AdditionalFields
        {
            get { return _additionalFields; }
            set { _additionalFields = value; }
        }

        /// <summary>
        /// The conversion pattern to use for the message body
        /// </summary>
        public string ConversionPattern { get; set; }

        /// <summary>
        /// If should append the stack trace to the message if the message is an exception
        /// </summary>
        public bool LogStackTraceFromMessage { get; set; }

        /// <summary>
        /// Specifies wehter location inormation should be included in the message
        /// </summary>
        public bool IncludeLocationInformation { get; set; }

        public bool SendTimeStampAsString { get; set; }

        public override void ActivateOptions()
        {
        }

        private Dictionary<string, object> ParseField(string value)
        {
            var innerAdditionalFields = new Dictionary<string, object>();

            if (value != null)
            {
                string[] fields;
                if (!string.IsNullOrEmpty(FieldSeparator))
                {
                    fields = value.Split(new[] { FieldSeparator }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    fields = value.Split(',');
                }

                if (!string.IsNullOrEmpty(KeyValueSeparator))
                {
                    innerAdditionalFields = fields
                        .Select(it => it.Split(new[] { KeyValueSeparator }, StringSplitOptions.RemoveEmptyEntries))
                        .ToDictionary(it => it[0], it => (object)it[1]);
                }
                else
                {
                    innerAdditionalFields = fields
                        .Select(it => it.Split(':'))
                        .ToDictionary(it => it[0], it => (object)it[1]);
                }
            }
            return innerAdditionalFields;
        }

        public override void Format(System.IO.TextWriter writer, LoggingEvent loggingEvent)
        {
            var gelfMessage = GetBaseGelfMessage(loggingEvent);

            AddLoggingEventToMessage(loggingEvent, gelfMessage);

            AddAdditionalFields(loggingEvent, gelfMessage);

            writer.Write(JsonConvert.SerializeObject(gelfMessage, Formatting.Indented));
        }

        private void AddLoggingEventToMessage(LoggingEvent loggingEvent, GelfMessage gelfMessage)
        {
            //If conversion pattern is specified then defer to PatterLayout for building the message body
            if (!string.IsNullOrWhiteSpace(ConversionPattern))
            {
                var message = GetValueFromPattern(loggingEvent, ConversionPattern);
                gelfMessage.FullMessage = message;
                gelfMessage.ShortMessage = message.TruncateMessage(SHORT_MESSAGE_LENGTH);
            }
            else //Otherwise do our custom message builder stuff
            {
                var messageObject = loggingEvent.MessageObject;
                if (messageObject == null)
                {
                    if (!string.IsNullOrEmpty(loggingEvent.RenderedMessage))
                    {
                        if (loggingEvent.RenderedMessage.ValidateJSON())
                        {
                            AddToMessage(gelfMessage, loggingEvent.RenderedMessage.ToJson().ToDictionary());
                        }

                        gelfMessage.FullMessage = !string.IsNullOrEmpty(gelfMessage.FullMessage) ? gelfMessage.FullMessage : loggingEvent.RenderedMessage;
                        gelfMessage.ShortMessage = !string.IsNullOrEmpty(gelfMessage.ShortMessage) ? gelfMessage.ShortMessage : gelfMessage.FullMessage.TruncateMessage(SHORT_MESSAGE_LENGTH);

                        return;
                    }
                    gelfMessage.FullMessage = SystemInfo.NullText;
                    gelfMessage.ShortMessage = SystemInfo.NullText;
                }

                if (messageObject is string || messageObject is SystemStringFormat)
                {
                    var fullMessage = messageObject.ToString();
                    gelfMessage.FullMessage = fullMessage;
                    gelfMessage.ShortMessage = fullMessage.TruncateMessage(SHORT_MESSAGE_LENGTH);
                }
                else if (messageObject is IDictionary)
                {
                    AddToMessage(gelfMessage, messageObject as IDictionary);
                }
                else
                {
                    AddToMessage(gelfMessage, messageObject.ToDictionary());
                }

                gelfMessage.FullMessage = !string.IsNullOrEmpty(gelfMessage.FullMessage) ? gelfMessage.FullMessage : messageObject.ToString();
                gelfMessage.ShortMessage = !string.IsNullOrEmpty(gelfMessage.ShortMessage) ? gelfMessage.ShortMessage : gelfMessage.FullMessage.TruncateMessage(SHORT_MESSAGE_LENGTH);
            }

            if (LogStackTraceFromMessage && loggingEvent.ExceptionObject != null)
            {
                gelfMessage.FullMessage = string.Format("{0} - {1}.", gelfMessage.FullMessage, loggingEvent.GetExceptionString());
            }
        }

        private GelfMessage GetBaseGelfMessage(LoggingEvent loggingEvent)
        {
            var message = new GelfMessage(SendTimeStampAsString)
            {
                Facility = Facility ?? "GELF",
                File = string.Empty,
                Host = HostName ?? Environment.MachineName,
                Level = GetSyslogSeverity(loggingEvent.Level),
                Line = string.Empty,
                TimeStamp = loggingEvent.TimeStamp,
                Version = GELF_VERSION,
            };

            message.Add("LoggerName", loggingEvent.LoggerName);

            if (this.IncludeLocationInformation)
            {
                message.File = (loggingEvent.LocationInformation.FileName != "?" ? loggingEvent.LocationInformation.FileName : string.Empty);
                message.Line = (loggingEvent.LocationInformation.LineNumber != "?" ? loggingEvent.LocationInformation.LineNumber : string.Empty);
            }

            return message;
        }

        private void AddToMessage(GelfMessage gelfMessage, IDictionary messageObject)
        {
            foreach (DictionaryEntry entry in messageObject as IDictionary)
            {
                var key = (entry.Key ?? string.Empty).ToString();
                var value = (entry.Value ?? string.Empty).ToString();
                if (FullMessageKeyValues.Contains(key, StringComparer.OrdinalIgnoreCase))
                    gelfMessage.FullMessage = value;
                else if (ShortMessageKeyValues.Contains(key, StringComparer.OrdinalIgnoreCase))
                    gelfMessage.ShortMessage = value.TruncateMessage(SHORT_MESSAGE_LENGTH);
                else
                {
                    key = key.StartsWith("_") ? key : "_" + key;
                    gelfMessage[key] = FormatAdditionalField(entry.Value);
                }
            }
        }

        private void AddAdditionalFields(LoggingEvent loggingEvent, GelfMessage message)
        {
            var additionalFields = ParseField(AdditionalFields) ?? new Dictionary<string, object>();
            foreach (KeyValuePair<string,object?> item in loggingEvent.GetProperties())
            {
                var key = item.Key;
                if (key != null && !key.StartsWith("log4net:") /*exclude log4net built-in properties */ )
                {
                    additionalFields.Add(key, FormatAdditionalField(item.Value));
                }
            }

            foreach (var kvp in additionalFields)
            {
                var key = kvp.Key.StartsWith("_") ? kvp.Key : "_" + kvp.Key;

                //If the value starts with a '%' then defer to the pattern layout
                var patternValue = kvp.Value as string;
                var value = patternValue != null && patternValue.StartsWith("%") ? GetValueFromPattern(loggingEvent, patternValue) : kvp.Value;
                message[key] = value;
            }
        }

        private object FormatAdditionalField(object value)
        {
            return value == null || value.GetType().IsNumeric() ? value : value.ToString();
        }

        private string GetValueFromPattern(LoggingEvent loggingEvent, string pattern)
        {
            //Reset the pattern layout
            _patternLayout.ConversionPattern = pattern;
            _patternLayout.ActivateOptions();

            //Write the results
            var sb = new StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
            {
                _patternLayout.Format(writer, loggingEvent);
                writer.Flush();
                return sb.ToString();
            }
        }

        private static long GetSyslogSeverity(Level level)
        {
            if (level == log4net.Core.Level.Alert)
                return (long)LocalSyslogAppender.SyslogSeverity.Alert;

            if (level == log4net.Core.Level.Critical || level == log4net.Core.Level.Fatal)
                return (long)LocalSyslogAppender.SyslogSeverity.Critical;

            if (level == log4net.Core.Level.Debug)
                return (long)LocalSyslogAppender.SyslogSeverity.Debug;

            if (level == log4net.Core.Level.Emergency)
                return (long)LocalSyslogAppender.SyslogSeverity.Emergency;

            if (level == log4net.Core.Level.Error)
                return (long)LocalSyslogAppender.SyslogSeverity.Error;

            if (level == log4net.Core.Level.Fine
                || level == log4net.Core.Level.Finer
                || level == log4net.Core.Level.Finest
                || level == log4net.Core.Level.Info
                || level == log4net.Core.Level.Off)
                return (long)LocalSyslogAppender.SyslogSeverity.Informational;

            if (level == log4net.Core.Level.Notice
                || level == log4net.Core.Level.Verbose
                || level == log4net.Core.Level.Trace)
                return (long)LocalSyslogAppender.SyslogSeverity.Notice;

            if (level == log4net.Core.Level.Severe)
                return (long)LocalSyslogAppender.SyslogSeverity.Emergency;

            if (level == log4net.Core.Level.Warn)
                return (long)LocalSyslogAppender.SyslogSeverity.Warning;

            return (long)LocalSyslogAppender.SyslogSeverity.Debug;
        }

        private static IEnumerable<string> FullMessageKeyValues = new[] { "FULLMESSAGE", "FULL_MESSAGE", "MESSAGE" };
        private static IEnumerable<string> ShortMessageKeyValues = new[] { "SHORTMESSAGE", "SHORT_MESSAGE", "MESSAGE" };
    }
}
