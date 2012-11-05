using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Esilog.Gelf4net.Appender;
using log4net.Core;
using System.Security.Cryptography;
using System.IO;
using NUnit.Framework;

namespace Gelf4netTest
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestFixture]
	public class Gelf4NetAppenderTest
	{
		private string graylogServerHost = "";
		
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
}