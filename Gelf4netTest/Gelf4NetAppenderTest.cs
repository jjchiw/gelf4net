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
			graylogServerHost = "192.168.1.102";
		}

		[Test()]
		public void AppendTest()
		{
			var gelfAppender = new Gelf4NetAppender();
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
			gelfAppender.GrayLogServerHost = graylogServerHost;
			gelfAppender.TestAppend(logEvent);

		}

		[Test()]
		public void AppendTestChunkMessage()
		{
			var gelfAppender = new Gelf4NetAppender();
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
			gelfAppender.GrayLogServerHost = graylogServerHost;
			gelfAppender.MaxChunkSize = 50;
			gelfAppender.AdditionalFields = "nombre:pedro,apellido:jimenez";
			logEvent.Properties["customProperty"] = "My Custom Property Woho";

			gelfAppender.TestAppend(logEvent);

		}

        [Test]
        public void AppendTestChunkMessageWithLoggerNameIncluded()
        {
            var gelfAppender = new Gelf4NetAppender();
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
            gelfAppender.GrayLogServerHost = graylogServerHost;
            gelfAppender.MaxChunkSize = 50;
            gelfAppender.AdditionalFields = "nombre:pedro,apellido:jimenez";
            gelfAppender.IncludeLoggerNameAsAdditionalField = true;
            logEvent.Properties["customProperty"] = "My Custom Property Woho";

            gelfAppender.TestAppend(logEvent);

        }

	   [Test()]
		public void TestMessageId()
		{
			Assert.AreEqual(GetMessageId().Length, 8);

		}
		
		public string GetMessageId()
		{
			var md5String = String.Join("", MD5.Create().ComputeHash(Encoding.Default.GetBytes("thousandsunny")).Select(it => it.ToString("x2")).ToArray<string>());
			var random = new Random((int)DateTime.Now.Ticks);
			var sb = new StringBuilder();
			var t = DateTime.Now.Ticks % 1000000000;
			var s = String.Format("{0}{1}", md5String.Substring(0, 10), md5String.Substring(20, 10));
			var r = random.Next(10000000).ToString("00000000");

			sb.Append(t);
			sb.Append(s);
			sb.Append(r);

			Console.WriteLine(t.ToString());
			Console.WriteLine(s.ToString());
			Console.WriteLine(r.ToString());
			Console.WriteLine(sb.ToString());

			var result = sb.ToString().Substring(0, 8);
			return result;

		}

		[Test()]
		public void TestCreateChunkMessage()
		{

			var result = new List<byte>();
			result.Add(Convert.ToByte(30));
			result.Add(Convert.ToByte(15));

			result.AddRange(Encoding.Default.GetBytes(GetMessageId()).ToArray<byte>());

			var indexShifted = (int)((uint)1 >> 8);
			var chunkCountShifted = (int)((uint)2 >> 8);

			result.Add(Convert.ToByte(indexShifted));
			result.Add(Convert.ToByte(1));

			result.Add(Convert.ToByte(chunkCountShifted));
			result.Add(Convert.ToByte(2));
			
			foreach (var item in result.ToArray<byte>()) {
				Console.WriteLine (item);
			}
			Console.WriteLine (result.ToArray<byte>());
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