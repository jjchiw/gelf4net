using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Esilog.Gelf4net.Appender;
using log4net.Core;
using System.Security.Cryptography;
using System.IO;
using NUnit.Framework;
using log4net;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gelf4netTest
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestFixture]
	public class Gelf4NetAppenderTest
	{
		private string graylogServerHost = "";
        private const int Port = 9743;
		
		[SetUpAttribute]
		public void Start()
		{
			//"public-graylog2.taulia.com"
			graylogServerHost = "10.0.1.64";
		}

		[Test()]
		public void AppendTest()
		{
			var gelfAppender = new TestGelf4NetAppenderWrapper();
            gelfAppender.GrayLogServerHost = graylogServerHost;
            gelfAppender.ActivateOptions();

            //def logEvent = new LoggingEvent(this.GetType().Name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, "Some Short Message", new Exception('Exception Message'))
			var data = new LoggingEventData
			{
				Domain = this.GetType().Name,
				Level = Level.Debug,
				LoggerName = "Tester",
				Message = "GrayLog4Net!!!",
				TimeStamp = DateTime.Now,
				UserName = "ElTesto"
			};

			var logEvent = new LoggingEvent(data);
			gelfAppender.TestAppend(logEvent);

		}


        [Test]
        public void VerifyAdditionalPropertiesAreAdded()
        {
            var gelfAppender = new TestGelf4NetAppenderWrapper();
            gelfAppender.GrayLogServerHost = "127.0.0.1";
            gelfAppender.GrayLogServerPort = Port;
            gelfAppender.ActivateOptions();

            ThreadContext.Properties["Test"] = 1;

            var data = new LoggingEventData
            {
                Domain = this.GetType().Name,
                Level = Level.Debug,
                LoggerName = "Tester",
                Message = "GrayLog4Net!!!",
                TimeStamp = DateTime.Now,
                UserName = "ElTesto"
            };

            var message = GetMessage(gelfAppender, new LoggingEvent(data));

            Assert.IsNotNull(message);
            Assert.IsTrue(message.AdditionalProperties.ContainsKey("_Test"));
            Assert.AreEqual(message.AdditionalProperties["_Test"], "1");
            Assert.IsFalse(message.AdditionalProperties.Keys.Any(x => x.StartsWith("log4net:")));
        }

        private static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
        private static GelfExtendedMessage GetMessage(TestGelf4NetAppenderWrapper appender, LoggingEvent logEvent)
        {
            GelfExtendedMessage message = null;
            IDictionary<string, string> additionalProps = new Dictionary<string, string>();
            var endPoint = new IPEndPoint(IPAddress.Any, Port);
            var client = new UdpClient(endPoint);
            client.BeginReceive(new AsyncCallback(result =>
            {
                UdpClient u = (UdpClient)((UdpState)(result.AsyncState)).Client;
                IPEndPoint e = (IPEndPoint)((UdpState)(result.AsyncState)).Endpoint;

                Byte[] receiveBytes = u.EndReceive(result, ref e);
                string receiveString = Encoding.UTF8.GetString(Decompress(receiveBytes));
                message  = GelfExtendedMessage.FromJson(receiveString);
            }), new UdpState
            {
                Endpoint = endPoint,
                Client = client
            });

            var stopWatch = Stopwatch.StartNew();

            appender.TestAppend(logEvent);

            while (message == null || stopWatch.ElapsedMilliseconds > 10000)
            {
                Thread.Sleep(100);
            }

            return message;
        }

		[Test()]
		public void AppendTestChunkMessage()
		{
			var gelfAppender = new TestGelf4NetAppenderWrapper();
            gelfAppender.GrayLogServerHost = graylogServerHost;
            gelfAppender.MaxChunkSize = 50;
            gelfAppender.AdditionalFields = "nombre:pedro,apellido:jimenez";
            gelfAppender.ActivateOptions();

            //def logEvent = new LoggingEvent(this.GetType().Name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, "Some Short Message", new Exception('Exception Message'))
			var data = new LoggingEventData
			{
				Domain = this.GetType().Name,
				Level = Level.Debug,
				LoggerName = "Big Tester",
				Message = LoremIpsum.Text,
				TimeStamp = DateTime.Now,
				UserName = "ElTesto"
			};

			var logEvent = new LoggingEvent(data);
			logEvent.Properties["customProperty"] = "My Custom Property Woho";

			gelfAppender.TestAppend(logEvent);
		}

		[Test()]
		public void TestSendMessageIteration()
		{
			var array = new List<int>{1,2,3,4,5,6,7,8,9};
			var max = 8;
			var iterations = array.Count / max + 1;

			for (int i = 0; i < iterations; i++)
			{
				array.Skip(i * max).Take(max).ToList<int>().ForEach(it =>{
					Console.WriteLine(it);
				});
				Console.WriteLine ("---");
			}
		}
	}

    public class UdpState
    {
        public IPEndPoint Endpoint { get; set; }
        public UdpClient Client { get; set; }
    }

    public class GelfExtendedMessage : GelfMessage
    {
        private IDictionary<string, string> _additionalProperties;
        public IDictionary<string, string> AdditionalProperties
        {
            get
            {
                if (_additionalProperties == null)
                    _additionalProperties = new Dictionary<string, string>();

                return _additionalProperties;
            }
            set
            {
                _additionalProperties = value;
            }
        }

        public static GelfExtendedMessage FromJson(string json)
        {
            var message = JsonConvert.DeserializeObject<GelfExtendedMessage>(json);
            foreach (var prop in JObject.Parse(json).Properties().Where(x => x.Name.StartsWith("_")))
                message.AdditionalProperties.Add(prop.Name, prop.Value.ToString());

            return message;
        }
    }
}