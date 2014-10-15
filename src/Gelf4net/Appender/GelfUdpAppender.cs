using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using log4net.Core;
using System.Text;
using System.Security.Cryptography;
using gelf4net.Util.TypeConverters;

namespace gelf4net.Appender
{
    /// <summary>
    /// Gelf Udp Appender
    /// </summary>
    public class GelfUdpAppender : log4net.Appender.UdpAppender
    {
        private const int MaxHeaderSize = 8;

        /// <summary>
        /// Gets or sets GrayLogServerHost.
        /// </summary>
        public string RemoteHostName { get; set; }

        public GelfUdpAppender()
        {
            Encoding = Encoding.UTF8;
            MaxChunkSize = 1024;

            log4net.Util.TypeConverters.ConverterRegistry.AddConverter(typeof(IPAddress), new IPAddressConverter());
        }

        public override void ActivateOptions()
        {
            if (RemoteAddress == null)
                RemoteAddress = IPAddress.Parse(GetIpAddressFromHostName());

            base.ActivateOptions();
        }

        protected override void InitializeClientConnection()
        {
            base.InitializeClientConnection();
        }

        public int MaxChunkSize { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {

                byte[] bytes = this.RenderLoggingEvent(loggingEvent).GzipMessage(this.Encoding);

                if (MaxChunkSize < bytes.Length)
                {
                    var chunkCount = (bytes.Length / MaxChunkSize) + 1;
                    var messageId = GenerateMessageId();
                    for (int i = 0; i < chunkCount; i++)
                    {
                        var messageChunkPrefix = CreateChunkedMessagePart(messageId, i, chunkCount);
                        var skip = i * MaxChunkSize;
                        var messageChunkSuffix = bytes.Skip(skip).Take(MaxChunkSize).ToArray<byte>();

                        var messageChunkFull = new byte[messageChunkPrefix.Length + messageChunkSuffix.Length];
                        messageChunkPrefix.CopyTo(messageChunkFull, 0);
                        messageChunkSuffix.CopyTo(messageChunkFull, messageChunkPrefix.Length);

                        Client.Send(messageChunkFull, messageChunkFull.Length, RemoteEndPoint);
                    }
                }
                else
                {
                    Client.Send(bytes, bytes.Length, RemoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                this.ErrorHandler.Error("Unable to send logging event to remote host " + this.RemoteAddress + " on port " + this.RemotePort + ".", ex, ErrorCode.WriteFailure);
            }
        }

        private string GetIpAddressFromHostName()
        {
            IPAddress[] addresslist = Dns.GetHostAddresses(RemoteHostName);
            return addresslist[0].ToString();
        }

        public static string GenerateMessageId()
        {
            var md5String = String.Join("", MD5.Create().ComputeHash(Encoding.Default.GetBytes(Environment.MachineName)).Select(it => it.ToString("x2")).ToArray<string>());
            var random = new Random((int)DateTime.Now.Ticks);
            var sb = new StringBuilder();
            var t = DateTime.Now.Ticks % 1000000000;
            var s = String.Format("{0}{1}", md5String.Substring(0, 10), md5String.Substring(20, 10));
            var r = random.Next(10000000).ToString("00000000");

            sb.Append(t);
            sb.Append(s);
            sb.Append(r);

            //Message ID: 8 bytes 
            return sb.ToString().Substring(0, MaxHeaderSize);
        }

        public static byte[] CreateChunkedMessagePart(string messageId, int index, int chunkCount)
        {
            var result = new List<byte>();
            var gelfHeader = new byte[2] { Convert.ToByte(30), Convert.ToByte(15) };
            result.AddRange(gelfHeader);
            result.AddRange(Encoding.Default.GetBytes(messageId).ToArray<byte>());
            result.Add(Convert.ToByte(index));
            result.Add(Convert.ToByte(chunkCount));

            return result.ToArray<byte>();
        }
    }
}