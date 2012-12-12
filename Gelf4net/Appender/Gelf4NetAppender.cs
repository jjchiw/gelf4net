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
using Esilog.Gelf4net.Layout;
using log4net.Util;

namespace Esilog.Gelf4net.Appender
{
    /// <summary>
    /// The gelf 4 net appender.
    /// </summary>
    public class Gelf4NetAppender : AppenderSkeleton
    {
        /// <summary>
        /// The transport.
        /// </summary>
        private GelfTransport _transport;

        /// <summary>
        /// The max chunk size of udp.
        /// </summary>
        private int _maxChunkSize;

        /// <summary>
        /// The name of the host machine
        /// </summary>
        public string Host { get; set; }

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
        /// Gets or sets a value indicating whether UseUdpTransport.
        /// </summary>
        public bool UseUdpTransport { get; set; }

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
            Layout = Layout ?? new GelfLayout();

            GrayLogServerHost = string.Empty;
            GrayLogServerHostIpAddress = string.Empty;
            GrayLogServerPort = 12201;
            MaxChunkSize = 1024;
            UseUdpTransport = true;
            GrayLogServerAmqpPort = 5672;
            GrayLogServerAmqpUser = "guest";
            GrayLogServerAmqpPassword = "guest";
            GrayLogServerAmqpVirtualHost = "/";
            GrayLogServerAmqpQueue = "queue1";
        }

        /// <summary>
        /// The activate options.
        /// </summary>
        public override void ActivateOptions()
        {
            _transport = UseUdpTransport
                             ? (GelfTransport) new UdpTransport(GetLoggingHostName(), GrayLogServerHostIpAddress, GrayLogServerPort) {MaxChunkSize = MaxChunkSize}
                             : (GelfTransport)new AmqpTransport(GetLoggingHostName(), GrayLogServerHostIpAddress, GrayLogServerPort)
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
            var message = RenderLoggingEvent(loggingEvent);

            _transport.Send(message);
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
                    ret = Environment.MachineName;
                }
            }

            return ret;
        }
    }
}