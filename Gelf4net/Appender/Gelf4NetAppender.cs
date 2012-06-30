using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Appender;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Esilog.Gelf4net.Transport;
using System.Collections;
using System.Text.RegularExpressions;

namespace Esilog.Gelf4net.Appender
{
	public class Gelf4NetAppender : AppenderSkeleton
	{
        public static string UNKNOWN_HOST = "unknown_host";
        private GelfTransport _transport;
		private string _additionalFields;
        private int _maxChunkSize;

		private Dictionary<string, string> innerAdditionalFields;

		public string AdditionalFields
		{
			get
			{
				return _additionalFields;
			}
			set
			{
				_additionalFields = value;

				if (_additionalFields != null)
					innerAdditionalFields = new Dictionary<string, string>();
				else
					innerAdditionalFields.Clear();
				innerAdditionalFields = _additionalFields.Split(',').ToDictionary(it => it.Split(':')[0], it => it.Split(':')[1]);
			}
		}
		public string Facility { get; set; }
		public string GrayLogServerHost { get; set; }
		public string GrayLogServerHostIpAddress { get; set; }
		public int GrayLogServerPort { get; set; }
		public string Host { get; set; }
		public bool IncludeLocationInformation { get; set; }
		public bool UseUdpTransport { get; set; }
		public int MaxChunkSize
		{
			get { return _maxChunkSize; }
			set 
			{ 
				_maxChunkSize = value;
				if(UseUdpTransport)
					((UdpTransport)_transport).MaxChunkSize = value;
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
			UseUdpTransport = true;
			GrayLogServerAmqpPort = 5672;
			GrayLogServerAmqpUser = "guest";
			GrayLogServerAmqpPassword = "guest";
			GrayLogServerAmqpVirtualHost = "/";
			GrayLogServerAmqpQueue = "queue1";
        }

        public override void ActivateOptions()
        {
            _transport = (UseUdpTransport)
                ? (GelfTransport)new UdpTransport { MaxChunkSize = MaxChunkSize }
                : (GelfTransport)new AmqpTransport
                {
                    VirtualHost = GrayLogServerAmqpVirtualHost,
                    User = GrayLogServerAmqpUser,
                    Password = GrayLogServerAmqpPassword,
                    Queue = GrayLogServerAmqpQueue
                };
		}

		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			String gelfJsonString = new GelfJsonBuilder().BuildFromLoggingEvent(
                loggingEvent, GetLoggingHostName(), Facility, IncludeLocationInformation, innerAdditionalFields);
			if (UseUdpTransport)
				SendGelfMessageToGrayLog(gelfJsonString);
			else
				SendAmqpMessageToGrayLog(gelfJsonString);
		}

		private void SendGelfMessageToGrayLog(string message)
		{
			if (GrayLogServerHostIpAddress == string.Empty)
				GrayLogServerHostIpAddress = GetIpAddressFromHostName();
			_transport.Send(GetLoggingHostName(), GrayLogServerHostIpAddress, GrayLogServerPort, message);
		}

		private void SendAmqpMessageToGrayLog(string message)
		{
			if (GrayLogServerHostIpAddress == string.Empty)
			{
				GrayLogServerHostIpAddress = GetIpAddressFromHostName();
			}
            _transport.Send(GetLoggingHostName(), GrayLogServerHostIpAddress, GrayLogServerAmqpPort, message);			
		}

        private String GetLoggingHostName()
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

        private string GetIpAddressFromHostName()
		{
			IPAddress[] addresslist = Dns.GetHostAddresses(GrayLogServerHost);
			return addresslist[0].ToString();
		}
	}
}
