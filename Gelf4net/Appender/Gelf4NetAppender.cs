using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Esilog.Gelf4net.Transport;
using log4net.Appender;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Esilog.Gelf4net.Appender
{
    /// <summary>
    /// The gelf 4 net appender.
    /// </summary>
    public class Gelf4NetAppender : AppenderSkeleton
    {
        /// <summary>
        /// The gelf version.
        /// </summary>
        private const string GELF_VERSION = "1.0";

        /// <summary>
        /// The short message length.
        /// </summary>
        private const int SHORT_MESSAGE_LENGTH = 250;

        /// <summary>
        /// The unknown host.
        /// </summary>
        public static string UNKNOWN_HOST = "unknown_host";

        /// <summary>
        /// The transport.
        /// </summary>
        private GelfTransport _transport;

        /// <summary>
        /// The additional fields.
        /// </summary>
        private string _additionalFields;

        /// <summary>
        /// The max chunk size of udp.
        /// </summary>
        private int _maxChunkSize;

        /// <summary>
        /// The additional fields configured in log4net config.
        /// </summary>
        private Dictionary<string, string> innerAdditionalFields;

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
        /// Gets or sets Facility.
        /// </summary>
        public string Facility { get; set; }

        /// <summary>
        /// Gets or sets GrayLogServerHost.
        /// </summary>
        public string GrayLogServerHost { get; set; }

        /// <summary>
        /// Gets or sets GrayLogServerHostIpAddress.
        /// </summary>
        public string GrayLogServerHostIpAddress { get; set; }

        /// <summary>
        /// Gets or sets GrayLogServerPort.
        /// </summary>
        public int GrayLogServerPort { get; set; }

        /// <summary>
        /// Gets or sets Host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IncludeLocationInformation.
        /// </summary>
        public bool IncludeLocationInformation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UseUdpTransport.
        /// </summary>
        public bool UseUdpTransport { get; set; }

        /// <summary>
        /// If should append the stack trace to the message.
        /// </summary>
        public bool LogStackTraceFromMessage { get; set; }

        /// <summary>
        /// The max chunk size of udp.
        /// </summary>
        public int MaxChunkSize
        {
            get { return _maxChunkSize; }
            set
            {
                _maxChunkSize = value;
                if (UseUdpTransport && _transport != null)
                {
                    ((UdpTransport) _transport).MaxChunkSize = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets GrayLogServerAmqpPort.
        /// </summary>
        public int GrayLogServerAmqpPort { get; set; }

        /// <summary>
        /// Gets or sets GrayLogServerAmqpUser.
        /// </summary>
        public string GrayLogServerAmqpUser { get; set; }

        /// <summary>
        /// Gets or sets GrayLogServerAmqpPassword.
        /// </summary>
        public string GrayLogServerAmqpPassword { get; set; }

        /// <summary>
        /// Gets or sets GrayLogServerAmqpVirtualHost.
        /// </summary>
        public string GrayLogServerAmqpVirtualHost { get; set; }

        /// <summary>
        /// Gets or sets GrayLogServerAmqpQueue.
        /// </summary>
        public string GrayLogServerAmqpQueue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gelf4NetAppender"/> class.
        /// </summary>
        public Gelf4NetAppender()
            : base()
        {
            Facility = null;
            GrayLogServerHost = string.Empty;
            GrayLogServerHostIpAddress = string.Empty;
            GrayLogServerPort = 12201;
            Host = null;
            IncludeLocationInformation = false;
            MaxChunkSize = 1024;
            UseUdpTransport = true;
            GrayLogServerAmqpPort = 5672;
            GrayLogServerAmqpUser = "guest";
            GrayLogServerAmqpPassword = "guest";
            GrayLogServerAmqpVirtualHost = "/";
            GrayLogServerAmqpQueue = "queue1";
            LogStackTraceFromMessage = true;
        }

        /// <summary>
        /// The activate options.
        /// </summary>
        public override void ActivateOptions()
        {
            _transport = UseUdpTransport
                             ? (GelfTransport) new UdpTransport {MaxChunkSize = MaxChunkSize}
                             : (GelfTransport) new AmqpTransport
                                                   {
                                                       VirtualHost = GrayLogServerAmqpVirtualHost, 
                                                       User = GrayLogServerAmqpUser, 
                                                       Password = GrayLogServerAmqpPassword, 
                                                       Queue = GrayLogServerAmqpQueue
                                                   };
        }

        /// <summary>Append a log event on graylog. </summary>
        /// <param name="loggingEvent">The logging event. </param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            GelfMessage gelfMessage = this.GetGelfMessage(loggingEvent);

            Dictionary<string, string> additionalFields = this.GetAdditionalFields(loggingEvent);

            string gelfJsonString = new GelfJsonBuilder().BuildFromLoggingEvent(
                gelfMessage, additionalFields);

            if (UseUdpTransport)
                SendGelfMessageToGrayLog(gelfJsonString);
            else
                SendAmqpMessageToGrayLog(gelfJsonString);
        }

        /// <summary>
        /// Get a GelfMessage with info about logging event.
        /// </summary>
        /// <param name="loggingEvent">Logging event</param>
        /// <returns>GelfMessage</returns>
        protected virtual GelfMessage GetGelfMessage(LoggingEvent loggingEvent)
        {

            var fullMessage = GetFullMessage(loggingEvent);

            var gelfMessage = new GelfMessage
            {
                Facility = Facility ?? "GELF",
                File = string.Empty,
                FullMesage = fullMessage,
                Host = this.GetLoggingHostName(),
                Level = this.GetSyslogSeverity(loggingEvent.Level),
                Line = string.Empty,
                ShortMessage = this.GetShortMessage(fullMessage, loggingEvent),
                TimeStamp = loggingEvent.TimeStamp,
                Version = GELF_VERSION,
            };

            if (this.IncludeLocationInformation)
            {
                gelfMessage.File = loggingEvent.LocationInformation.FileName;
                gelfMessage.Line = loggingEvent.LocationInformation.LineNumber;
            }

            return gelfMessage;
        }

        /// <summary>
        /// The additional fields.
        /// Concate the additional fields specified in log4net config (<see cref="AdditionalFields"/>) and the LoggingEvent.Properties
        /// </summary>
        /// <param name="loggingEvent">The logging event. </param>
        /// <returns>Dictionary with the additional fields</returns>
        protected virtual Dictionary<string, string> GetAdditionalFields(LoggingEvent loggingEvent)
        {
            Dictionary<string, string> additionalFields = innerAdditionalFields == null
                                                              ? new Dictionary<string, string>()
                                                              : new Dictionary<string, string>(innerAdditionalFields);

            if (loggingEvent.Properties != null)
            {
                foreach (DictionaryEntry item in loggingEvent.Properties)
                {
                    var key = item.Key as string;
                    if (key != null)
                    {
                        additionalFields.Add(key, item.Value as string);
                    }
                }
            }

            return additionalFields;
        }

        /// <summary>
        /// Short message.
        /// Truncate the message to 250 character.
        /// </summary>
        /// <param name="fullMessage">The full message. </param>
        /// <param name="loggingEvent">logging event. </param>
        /// <returns>The get short message. </returns>
        protected virtual string GetShortMessage(string fullMessage, LoggingEvent loggingEvent)
        {
            return (fullMessage.Length > SHORT_MESSAGE_LENGTH)
                       ? fullMessage.Substring(0, SHORT_MESSAGE_LENGTH - 1)
                       : fullMessage;
        }

        /// <summary>
        /// Get full message.
        /// If a Layout was defined, the full message will use that.
        /// Append the log exception and stacktrace if <see cref="LogStackTraceFromMessage"/> is true.
        /// </summary>
        /// <param name="loggingEvent">logging event. </param>
        /// <returns>Full message. </returns>
        protected virtual string GetFullMessage(LoggingEvent loggingEvent)
        {
            if (this.Layout != null)
            {
                return this.RenderLoggingEvent(loggingEvent);
            }

            string fullMessage = loggingEvent.RenderedMessage;

            if (this.LogStackTraceFromMessage && loggingEvent.ExceptionObject != null)
            {
                fullMessage = string.Format("{0} - {1}.", fullMessage, loggingEvent.GetExceptionString());
            }

            return fullMessage;
        }

        /// <summary>
        /// Send gelf message to graylog.
        /// </summary>
        /// <param name="message">The message. </param>
        private void SendGelfMessageToGrayLog(string message)
        {
            if (GrayLogServerHostIpAddress == string.Empty)
                GrayLogServerHostIpAddress = GetIpAddressFromHostName();
            _transport.Send(GetLoggingHostName(), GrayLogServerHostIpAddress, GrayLogServerPort, message);
        }

        /// <summary>
        /// Send amqp message to graylog.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SendAmqpMessageToGrayLog(string message)
        {
            if (GrayLogServerHostIpAddress == string.Empty)
            {
                GrayLogServerHostIpAddress = GetIpAddressFromHostName();
            }

            _transport.Send(GetLoggingHostName(), GrayLogServerHostIpAddress, GrayLogServerAmqpPort, message);
        }

        /// <summary>
        /// Get logging host name.
        /// </summary>
        /// <returns>logging host name. </returns>
        private string GetLoggingHostName()
        {
            string ret = Host;
            if (ret == null)
            {
                try
                {
                    ret = System.Net.Dns.GetHostName();
                }
                catch (SocketException)
                {
                    ret = UNKNOWN_HOST;
                }
            }

            return ret;
        }

        /// <summary>
        /// Get ip address from host name.
        /// </summary>
        /// <returns>Ip address from host name. </returns>
        private string GetIpAddressFromHostName()
        {
            IPAddress[] addresslist = Dns.GetHostAddresses(GrayLogServerHost);
            return addresslist[0].ToString();
        }

        /// <summary>
        /// Get syslog severity.
        /// </summary>
        /// <param name="level">The level. </param>
        /// <returns> The syslog severity. </returns>
        private int GetSyslogSeverity(Level level)
        {
            if (level == log4net.Core.Level.Alert)
                return (int) LocalSyslogAppender.SyslogSeverity.Alert;

            if (level == log4net.Core.Level.Critical || level == log4net.Core.Level.Fatal)
                return (int) LocalSyslogAppender.SyslogSeverity.Critical;

            if (level == log4net.Core.Level.Debug)
                return (int) LocalSyslogAppender.SyslogSeverity.Debug;

            if (level == log4net.Core.Level.Emergency)
                return (int) LocalSyslogAppender.SyslogSeverity.Emergency;

            if (level == log4net.Core.Level.Error)
                return (int) LocalSyslogAppender.SyslogSeverity.Error;

            if (level == log4net.Core.Level.Fine
                || level == log4net.Core.Level.Finer
                || level == log4net.Core.Level.Finest
                || level == log4net.Core.Level.Info
                || level == log4net.Core.Level.Off)
                return (int) LocalSyslogAppender.SyslogSeverity.Informational;

            if (level == log4net.Core.Level.Notice
                || level == log4net.Core.Level.Verbose
                || level == log4net.Core.Level.Trace)
                return (int) LocalSyslogAppender.SyslogSeverity.Notice;

            if (level == log4net.Core.Level.Severe)
                return (int) LocalSyslogAppender.SyslogSeverity.Emergency;

            if (level == log4net.Core.Level.Warn)
                return (int) LocalSyslogAppender.SyslogSeverity.Warning;

            return (int) LocalSyslogAppender.SyslogSeverity.Debug;
        }
    }
}