using gelf4net.Appender;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace gelf4net.Layout
{
    public class GelfLayout : LayoutSkeleton
    {
        private const string GELF_VERSION = "1.0";
        private const int SHORT_MESSAGE_LENGTH = 250;
        private Dictionary<string, string> innerAdditionalFields;
        private string _additionalFields;

        public GelfLayout()
        {
            Facility = Facility ?? "Gelf";
            IncludeLocationInformation = false;
            LogStackTraceFromMessage = true;
            IgnoresException = false;
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
        /// The additional fields configured in log4net config.
        /// </summary>
        public string AdditionalFields
        {
            get { return _additionalFields; }
            set
            {
                _additionalFields = value;

                if (_additionalFields != null)
                    innerAdditionalFields = new Dictionary<string, string>();
                else
                    innerAdditionalFields.Clear();
                innerAdditionalFields = _additionalFields.Split(',').ToDictionary(it => it.Split(':')[0],
                                                                                  it => it.Split(':')[1]);
            }
        }

        /// <summary>
        /// If should append the stack trace to the message if the message is an exception
        /// </summary>
        public bool LogStackTraceFromMessage { get; set; }

        /// <summary>
        /// Specifies wehter location inormation should be included in the message
        /// </summary>
        public bool IncludeLocationInformation { get; set; }

        public override void ActivateOptions()
        {
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
            var messageObject = loggingEvent.MessageObject;
            if (messageObject == null)
            {
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

            gelfMessage.FullMessage = gelfMessage.FullMessage ?? messageObject.ToString();
            gelfMessage.ShortMessage = gelfMessage.ShortMessage ?? gelfMessage.FullMessage.TruncateMessage(SHORT_MESSAGE_LENGTH);

            if (LogStackTraceFromMessage && loggingEvent.ExceptionObject != null)
            {
                gelfMessage.FullMessage = string.Format("{0} - {1}.", gelfMessage.FullMessage, loggingEvent.GetExceptionString());
            }
        }

        private GelfMessage GetBaseGelfMessage(LoggingEvent loggingEvent)
        {
            var message = new GelfMessage
            {
                Facility = Facility ?? "GELF",
                File = string.Empty,
                Host = Environment.MachineName,
                Level = GetSyslogSeverity(loggingEvent.Level),
                Line = string.Empty,
                TimeStamp = loggingEvent.TimeStamp,
                Version = GELF_VERSION,
            };

            message.Add("LoggerName", loggingEvent.LoggerName);

            if (this.IncludeLocationInformation)
            {
                message.File = loggingEvent.LocationInformation.FileName;
                message.Line = loggingEvent.LocationInformation.LineNumber;
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
                    gelfMessage[key] = value;
                }
            }
        }

        private void AddAdditionalFields(LoggingEvent loggingEvent, GelfMessage message)
        {
            Dictionary<string, string> additionalFields = innerAdditionalFields == null
                                                  ? new Dictionary<string, string>()
                                                  : new Dictionary<string, string>(innerAdditionalFields);

            foreach (DictionaryEntry item in loggingEvent.GetProperties())
            {
                var key = item.Key as string;
                if (key != null && !key.StartsWith("log4net:") /*exclude log4net built-in properties */ )
                {
                    var val = item.Value == null ? null : item.Value.ToString();
                    additionalFields.Add(key, val);
                }
            }

            foreach (var kvp in additionalFields)
            {
                var key = kvp.Key.StartsWith("_") ? kvp.Key : "_" + kvp.Key;
                message[key] = kvp.Value;
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
