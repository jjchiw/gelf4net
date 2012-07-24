using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using log4net.Appender;
using log4net.Core;

namespace Esilog.Gelf4net.Appender
{
    class GelfJsonBuilder
    {
        private static int SHORT_MESSAGE_LENGTH = 250;
        private const string GELF_VERSION = "1.0";

        internal string BuildFromLoggingEvent(string message, log4net.Core.LoggingEvent loggingEvent, string hostName, string facility, bool isConfiguredToIncludeLocationInformation, Dictionary<string, string> innerAdditionalFields)
        {
            var fullMessage = GetFullMessage(message, loggingEvent);
            var gelfMessage = new GelfMessage
            {
                Facility = (facility ?? "GELF"),
                File = "",
                FullMesage = fullMessage,
                Host = hostName,
                Level = GetSyslogSeverity(loggingEvent.Level),
                Line = "",
                ShortMessage = GetShortMessage(fullMessage),
                TimeStamp = loggingEvent.TimeStamp,
                Version = GELF_VERSION,
            };

            if (isConfiguredToIncludeLocationInformation)
            {
                gelfMessage.File = loggingEvent.LocationInformation.FileName;
                gelfMessage.Line = loggingEvent.LocationInformation.LineNumber;
            }

            return GetGelfJsonMessage(loggingEvent, innerAdditionalFields, gelfMessage);
        }

        private string GetFullMessage(string message, log4net.Core.LoggingEvent loggingEvent)
        {
            var fullMessage = message;
            if (loggingEvent.ExceptionObject != null)
            {
                fullMessage = String.Format("{0} - {1}. {2}. {3}.", fullMessage, loggingEvent.ExceptionObject.Source, loggingEvent.ExceptionObject.Message, loggingEvent.ExceptionObject.StackTrace);
            }
            return fullMessage;
        }

        private static string GetShortMessage(string fullMessage)
        {
            return (fullMessage.Length > SHORT_MESSAGE_LENGTH)
                ? fullMessage.Substring(0, SHORT_MESSAGE_LENGTH - 1)
                : fullMessage;
        }

        private string GetGelfJsonMessage(log4net.Core.LoggingEvent loggingEvent, Dictionary<string, string> innerAdditionalFields, GelfMessage gelfMessage)
        {
            var gelfJsonMessage = JsonConvert.SerializeObject(gelfMessage);
            var jsonObject = JObject.Parse(gelfJsonMessage);
            AddInnerAdditionalFields(jsonObject, innerAdditionalFields);
            AddLoggingEventAdditionalFields(jsonObject, loggingEvent);
            return jsonObject.ToString();
        }

        private void AddInnerAdditionalFields(JObject jsonObject, Dictionary<string, string> innerAdditionalFields)
        {
            if (innerAdditionalFields == null) return;
            foreach (var item in innerAdditionalFields)
            {
                AddAdditionalFields(item.Key, item.Value, jsonObject);
            }
        }

        private void AddLoggingEventAdditionalFields(JObject jsonObject, LoggingEvent loggingEvent)
        {
            if (loggingEvent.Properties == null) return;
            foreach (DictionaryEntry item in loggingEvent.Properties)
            {
                var key = item.Key as string;
                if (key != null)
                {
                    AddAdditionalFields(key, item.Value as string, jsonObject);
                }
            }
        }

        private void AddAdditionalFields(string key, string value, JObject jsonObject)
        {
            if (key == null) return;

            if (!key.StartsWith("_"))
                key = String.Format("_{0}", key);

            if (key != "_id")
            {
                key = Regex.Replace(key, "[\\W]", "");
                jsonObject.Add(key, value);
            }
        }
        private int GetSyslogSeverity(log4net.Core.Level level)
        {
            if (level == log4net.Core.Level.Alert)
                return (int)LocalSyslogAppender.SyslogSeverity.Alert;

            if (level == log4net.Core.Level.Critical || level == log4net.Core.Level.Fatal)
                return (int)LocalSyslogAppender.SyslogSeverity.Critical;

            if (level == log4net.Core.Level.Debug)
                return (int)LocalSyslogAppender.SyslogSeverity.Debug;

            if (level == log4net.Core.Level.Emergency)
                return (int)LocalSyslogAppender.SyslogSeverity.Emergency;

            if (level == log4net.Core.Level.Error)
                return (int)LocalSyslogAppender.SyslogSeverity.Error;

            if (level == log4net.Core.Level.Fine
                || level == log4net.Core.Level.Finer
                || level == log4net.Core.Level.Finest
                || level == log4net.Core.Level.Info
                || level == log4net.Core.Level.Off)
                return (int)LocalSyslogAppender.SyslogSeverity.Informational;

            if (level == log4net.Core.Level.Notice
                || level == log4net.Core.Level.Verbose
                || level == log4net.Core.Level.Trace)
                return (int)LocalSyslogAppender.SyslogSeverity.Notice;

            if (level == log4net.Core.Level.Severe)
                return (int)LocalSyslogAppender.SyslogSeverity.Emergency;

            if (level == log4net.Core.Level.Warn)
                return (int)LocalSyslogAppender.SyslogSeverity.Warning;

            return (int)LocalSyslogAppender.SyslogSeverity.Debug;
        }

    }
}
