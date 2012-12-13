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
using log4net.Appender;
using log4net.Repository;
using log4net.Util;
using System.Globalization;
using Esilog.Gelf4net;

namespace Gelf4netTest
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestFixture]
	public class Gelf4NetAppenderTest
	{
        //private const string GrayLogServerHost = "localhost";

        //[Test()]
        //public void AppendTest()
        //{
        //    var gelfAppender = new TestGelf4NetAppenderWrapper();
        //    gelfAppender.GrayLogServerHost = GrayLogServerHost;
        //    gelfAppender.ActivateOptions();

        //    //def logEvent = new LoggingEvent(this.GetType().Name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, "Some Short Message", new Exception('Exception Message'))
        //    var data = new LoggingEventData
        //    {
        //        Domain = this.GetType().Name,
        //        Level = Level.Debug,
        //        LoggerName = "Tester",
        //        Message = "GrayLog4Net!!!",
        //        TimeStamp = DateTime.Now,
        //        UserName = "ElTesto"
        //    };

        //    var logEvent = new LoggingEvent(data);
        //    gelfAppender.TestAppend(logEvent);

        //}

        //[Test]
        //public void VerifyAdditionalPropertiesAreAdded()
        //{
        //    var appender = GetAppender();

        //    ThreadContext.Properties["Test"] = 1;

        //    var data = new LoggingEventData
        //    {
        //        Domain = this.GetType().Name,
        //        Level = Level.Debug,
        //        LoggerName = "Tester",
        //        Message = "GrayLog4Net!!!",
        //        TimeStamp = DateTime.Now,
        //        UserName = "ElTesto"
        //    };

        //    var message = GetMessage(appender, new LoggingEvent(data));

        //    Assert.IsNotNull(message);
        //    Assert.IsTrue(message.ContainsKey("_Test"));
        //    Assert.AreEqual(message["_Test"], "1");
        //    Assert.IsFalse(message.Keys.Any(x => x.StartsWith("log4net:")));
        //}

        //[Test]
        //public void StringMessageShouldBeLogged()
        //{
        //    var message = "This is a test message!!!";
        //    var loggingEvent = GetLogginEvent(message);

        //    var result = GetMessage(GetAppender(), loggingEvent);

        //    Assert.AreEqual(message, result.FullMessage);
        //    Assert.AreEqual(message, result.ShortMessage);
        //}

        //[Test]
        //public void StringFormattedMessagesShouldBeLogged()
        //{
        //    var message = new SystemStringFormat(CultureInfo.CurrentCulture, "This is a {0}", "test");
        //    var loggingEvent = GetLogginEvent(message);

        //    var result = GetMessage(GetAppender(), loggingEvent);

        //    Assert.AreEqual(message.ToString(), result.FullMessage);
        //    Assert.AreEqual(message.ToString(), result.ShortMessage);
        //}

        //private static Gelf4NetAppender GetAppender()
        //{
        //    var gelfAppender = new Gelf4NetAppender();
        //    gelfAppender.GrayLogServerHost = "127.0.0.1";
        //    gelfAppender.GrayLogServerPort = Port;
        //    gelfAppender.ActivateOptions();

        //    return gelfAppender;
        //}
        //private static LoggingEvent GetLogginEvent(object message)
        //{
        //    return new LoggingEvent((Type)null, (ILoggerRepository)null, null, Level.Debug, message, null);
        //}

        //private static byte[] Decompress(byte[] gzip)
        //{
        //    // Create a GZIP stream with decompression mode.
        //    // ... Then create a buffer and write into while reading from the GZIP stream.
        //    using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
        //    {
        //        const int size = 4096;
        //        byte[] buffer = new byte[size];
        //        using (MemoryStream memory = new MemoryStream())
        //        {
        //            int count = 0;
        //            do
        //            {
        //                count = stream.Read(buffer, 0, size);
        //                if (count > 0)
        //                {
        //                    memory.Write(buffer, 0, count);
        //                }
        //            }
        //            while (count > 0);
        //            return memory.ToArray();
        //        }
        //    }
        //}
        //private static GelfMessage GetMessage(Gelf4NetAppender appender, LoggingEvent logEvent)
        //{
        //    GelfMessage message = null;
        //    IDictionary<string, string> additionalProps = new Dictionary<string, string>();
        //    var endPoint = new IPEndPoint(IPAddress.Any, 0);
        //    var client = new UdpClient(endPoint);
        //    var port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
        //    appender.GrayLogServerPort = port;
        //    client.BeginReceive(new AsyncCallback(result =>
        //    {
        //        UdpClient u = (UdpClient)((UdpState)(result.AsyncState)).Client;
        //        IPEndPoint e = (IPEndPoint)((UdpState)(result.AsyncState)).Endpoint;

        //        Byte[] receiveBytes = u.EndReceive(result, ref e);
        //        string receiveString = Encoding.UTF8.GetString(Decompress(receiveBytes));
        //        message = JsonConvert.DeserializeObject<GelfMessage>(receiveString);
        //    }), new UdpState
        //    {
        //        Endpoint = endPoint,
        //        Client = client
        //    });

        //    var stopWatch = Stopwatch.StartNew();

        //    appender.DoAppend(logEvent);

        //    while (message == null || stopWatch.ElapsedMilliseconds > 10000)
        //    {
        //        Thread.Sleep(100);
        //    }

        //    return message;
        //}

        //[Test()]
        //public void AppendTestChunkMessage()
        //{
        //    var gelfAppender = new TestGelf4NetAppenderWrapper();
        //    gelfAppender.GrayLogServerHost = graylogServerHost;
        //    gelfAppender.MaxChunkSize = 50;
        //    gelfAppender.AdditionalFields = "nombre:pedro,apellido:jimenez";
        //    gelfAppender.ActivateOptions();

        //    //def logEvent = new LoggingEvent(this.GetType().Name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, "Some Short Message", new Exception('Exception Message'))
        //    var data = new LoggingEventData
        //    {
        //        Domain = this.GetType().Name,
        //        Level = Level.Debug,
        //        LoggerName = "Big Tester",
        //        Message = LoremIpsum.Text,
        //        TimeStamp = DateTime.Now,
        //        UserName = "ElTesto"
        //    };

        //    var logEvent = new LoggingEvent(data);
        //    logEvent.Properties["customProperty"] = "My Custom Property Woho";

        //    gelfAppender.TestAppend(logEvent);
        //}
	}

    public class UdpState
    {
        public IPEndPoint Endpoint { get; set; }
        public UdpClient Client { get; set; }
    }
}