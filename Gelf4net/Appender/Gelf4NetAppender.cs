using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.IO.Compression;
using RabbitMQ.Client;
using Esilog.Gelf4net.Transport;
using System.Collections;
using System.Text.RegularExpressions;

namespace Esilog.Gelf4net.Appender
{
	public class Gelf4NetAppender : AppenderSkeleton
	{
		public static int SHORT_MESSAGE_LENGTH = 250;
		public static string UNKNOWN_HOST = "unknown_host";
		private static string GELF_VERSION = "1.0";

		private UdpTransport udpTransport;
		private AmqpTransport amqpTransport;

		private string additionalFields;

		//---------------------------------------
		//configuration settings for the appender
		private Dictionary<string, string> innerAdditionalFields;

		public string AdditionalFields
		{
			get
			{
				return additionalFields;
			}
			set
			{
				additionalFields = value;

				if (additionalFields != null)
				{
					innerAdditionalFields = new Dictionary<string, string>();
				}
				else
				{
					innerAdditionalFields.Clear();
				}
				innerAdditionalFields = additionalFields.Split(',').ToDictionary(it => it.Split(':')[0], it => it.Split(':')[1]);
			}
		}
		public string Facility { get; set; }
		public string GrayLogServerHost { get; set; }
		public string GrayLogServerHostIpAddress { get; set; }
		public int GrayLogServerPort { get; set; }
		public string Host { get; set; }
		public bool IncludeLocationInformation { get; set; }
		public bool SendAsGelfOrAmqp { get; set; }
        public bool IncludeLoggerNameAsAdditionalField { get; set; }

		private int maxChunkSize;

		public int MaxChunkSize
		{
			get { return maxChunkSize; }
			set 
			{ 
				maxChunkSize = value;
				if(udpTransport != null)
					udpTransport.MaxChunkSize = value;
			}
		}


		public int GrayLogServerAmqpPort { get; set; }
		public string GrayLogServerAmqpUser { get; set; }
		public string GrayLogServerAmqpPassword { get; set; }
		public string GrayLogServerAmqpVirtualHost { get; set; }
		public string GrayLogServerAmqpQueue { get; set; }


		public Gelf4NetAppender()
			: base()
		{
			Facility = null;
			GrayLogServerHost = "";
			GrayLogServerHostIpAddress = "";
			GrayLogServerPort = 12201;
			Host = null;
			IncludeLocationInformation = false;
			MaxChunkSize = 1024;
			SendAsGelfOrAmqp = true;
		    IncludeLoggerNameAsAdditionalField = false;


			GrayLogServerAmqpPort = 5672;
			GrayLogServerAmqpUser = "guest";
			GrayLogServerAmqpPassword = "guest";
			GrayLogServerAmqpVirtualHost = "/";
			GrayLogServerAmqpQueue = "queue1";

			udpTransport = new UdpTransport
			{
				MaxChunkSize = MaxChunkSize
			};

			amqpTransport = new AmqpTransport
			{
				VirtualHost = GrayLogServerAmqpVirtualHost,
				User = GrayLogServerAmqpUser,
				Password = GrayLogServerAmqpPassword,
				Queue = GrayLogServerAmqpQueue
			};
		}

		
		/// <summary>
		/// Test porpuse used only in the unit test
		/// </summary>
		/// <param name="loggingEvent"></param>
		public void TestAppend(log4net.Core.LoggingEvent loggingEvent)
		{
			Append(loggingEvent);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loggingEvent"></param>
		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			String gelfJsonString = CreateGelfJsonFromLoggingEvent(loggingEvent);
			if (SendAsGelfOrAmqp)
				SendGelfMessageToGrayLog(gelfJsonString);
			else
				SendAmqpMessageToGrayLog(gelfJsonString);
		}

		/// <summary>
		/// Get the HostName
		/// </summary>
		private String LoggingHostName
		{
			get
			{
				String ret = Host;
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
		}

		/// <summary>
		/// Get the UDP address of the Server with the format "udp://{GrayLogServerHost}:{GrayLogServerPort}"
		/// </summary>
		private string GrayLogServerUrl
		{
			get
			{
				return String.Format("udp://{0}:{1}", GrayLogServerHost, GrayLogServerPort);
			}
		}

		/// <summary>
		/// Sending the message to the graylog2 server via UDP
		/// </summary>
		/// <param name="message"></param>
		private void SendGelfMessageToGrayLog(string message)
		{
			if (GrayLogServerHostIpAddress == string.Empty)
			{
				GrayLogServerHostIpAddress = GetIpAddressFromHostName();
			}
			udpTransport.Send(LoggingHostName, GrayLogServerHostIpAddress, GrayLogServerPort, message);
		}

		/// <summary>
		/// Sending the message to the graylog2 server via AMQP
		/// </summary>
		/// <param name="message">Message to be sent</param>
		private void SendAmqpMessageToGrayLog(string message)
		{
			if (GrayLogServerHostIpAddress == string.Empty)
			{
				GrayLogServerHostIpAddress = GetIpAddressFromHostName();
			}
			amqpTransport.Send(LoggingHostName, GrayLogServerHostIpAddress, GrayLogServerPort, message);
			
		}

		/// <summary>
		/// Get the first IPAddress from the HostName
		/// </summary>
		/// <returns>IPAddress as string</returns>
		private string GetIpAddressFromHostName()
		{
			IPAddress[] addresslist = Dns.GetHostAddresses(GrayLogServerHost);
			return addresslist[0].ToString();
		}

		

		/// <summary>
		/// Creates the JSON String for a given <code>LoggingEvent</code>.
		/// The "short_message" of the GELF message is max 50 chars long.
		/// Message building and skipping of additional fields etc is based on
		/// https://github.com/Graylog2/graylog2-docs/wiki/GELF from Jan 7th 2011.
		/// </summary>
		/// <param name="loggingEvent"> The logging event to base the JSON creation on</param>
		/// <returns>GelfMessage as JSON</returns>
		private string CreateGelfJsonFromLoggingEvent(log4net.Core.LoggingEvent loggingEvent)
		{
			var fullMessage = loggingEvent.RenderedMessage;
			if (loggingEvent.ExceptionObject != null)
			{
				fullMessage = String.Format("{0} - {1}. {2}. {3}.", fullMessage, loggingEvent.ExceptionObject.Source, loggingEvent.ExceptionObject.Message, loggingEvent.ExceptionObject.StackTrace);
			}

			var shortMessage = fullMessage;

			if (shortMessage.Length > SHORT_MESSAGE_LENGTH)
			{
				shortMessage = shortMessage.Substring(0, SHORT_MESSAGE_LENGTH - 1);
			}

			var gelfMessage = new GelfMessage
			{
				Facility = (this.Facility ?? "GELF"),
				File = "",
				FullMesage = fullMessage,
				Host = LoggingHostName,
				Level = GetSyslogSeverity(loggingEvent.Level),
				Line = "",
				ShortMessage = shortMessage,
				TimeStamp = loggingEvent.TimeStamp,
				Version = GELF_VERSION
			};

			//only set location information if configured
			if (IncludeLocationInformation)
			{
				gelfMessage.File = loggingEvent.LocationInformation.FileName;
				gelfMessage.Line = loggingEvent.LocationInformation.LineNumber;
			}

			var gelfJsonMessage = JsonConvert.SerializeObject(gelfMessage);

			var jsonObject = JObject.Parse(gelfJsonMessage);

			//add additional fields and prepend with _ if not present already
			if (innerAdditionalFields != null)
			{
				foreach (var item in innerAdditionalFields)
				{
					AddAdditionalFields(item.Key, item.Value, jsonObject);
				}
			}

			//add additional fields and prepend with _ if not present already
			if (loggingEvent.Properties != null)
			{
				foreach (DictionaryEntry item in loggingEvent.Properties)
				{
					var key = item.Key as string;
					if (key != null)
					{
						AddAdditionalFields(key, item.Value as string, jsonObject);
					}
				}
			}

            if(IncludeLoggerNameAsAdditionalField)
            {
                AddAdditionalFields("_logger_name", loggingEvent.LoggerName, jsonObject);
            }

			return jsonObject.ToString();
		}

		/// <summary>
		/// Add    
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="json"></param>
		private void AddAdditionalFields(string key, string value, JObject jsonObject)
		{
			if (key != null)
			{
				if (!key.StartsWith("_"))
				{
					key = String.Format("_{0}", key);
				}

				if (key != "_id")
				{
					key = Regex.Replace(key, "[\\W]", "");
					jsonObject.Add(key, value);
				}
			}
		}

		/// <summary>
		/// Convert the log4net level to SyslogSeverity
		/// </summary>
		/// <param name="level">log4net level</param>
		/// <returns>SyslogSeverity as integer</returns>
		private int GetSyslogSeverity(log4net.Core.Level level)
		{
			if(level == log4net.Core.Level.Alert)
				return (int) LocalSyslogAppender.SyslogSeverity.Alert;

			if(level == log4net.Core.Level.Critical || level == log4net.Core.Level.Fatal)
				return (int)LocalSyslogAppender.SyslogSeverity.Critical;

			if(level == log4net.Core.Level.Debug)
				return (int)LocalSyslogAppender.SyslogSeverity.Debug;

			if(level == log4net.Core.Level.Emergency)
				return (int)LocalSyslogAppender.SyslogSeverity.Emergency;

			if(level == log4net.Core.Level.Error)
				return (int)LocalSyslogAppender.SyslogSeverity.Error;

			if(level == log4net.Core.Level.Fine 
				|| level == log4net.Core.Level.Finer 
				|| level ==  log4net.Core.Level.Finest 
				|| level ==  log4net.Core.Level.Info 
				|| level ==  log4net.Core.Level.Off)
				return (int)LocalSyslogAppender.SyslogSeverity.Informational;

			if(level == log4net.Core.Level.Notice 
				|| level == log4net.Core.Level.Verbose 
				|| level == log4net.Core.Level.Trace)
				return (int) LocalSyslogAppender.SyslogSeverity.Notice;

			if(level == log4net.Core.Level.Severe)
				return (int) LocalSyslogAppender.SyslogSeverity.Emergency;
			
			if(level == log4net.Core.Level.Warn)
				return (int) LocalSyslogAppender.SyslogSeverity.Warning;

			return (int) LocalSyslogAppender.SyslogSeverity.Debug;
		}

	}

	/// <summary>
	/// GelfMessage Class
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	class GelfMessage
	{
		[JsonProperty("facility")]
		public string Facility { get; set; }

		[JsonProperty("file")]
		public string File { get; set; }

		[JsonProperty("full_message")]
		public string FullMesage { get; set; }

		[JsonProperty("host")]
		public string Host { get; set; }

		[JsonProperty("level")]
		public int Level { get; set; }

		[JsonProperty("line")]
		public string Line { get; set; }

		[JsonProperty("short_message")]
		public string ShortMessage { get; set; }

		[JsonProperty("timestamp")]
		public DateTime TimeStamp { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }
	}

}
