using gelf4net.Layout;
using gelf4net;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Util;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;

namespace Gelf4netTest.Layout
{
    [TestFixture]
    public class GelfLayoutTests
    {
        [Test]
        public void StringFormat()
        {
            var layout = new GelfLayout();

            var message = new SystemStringFormat(CultureInfo.CurrentCulture, "This is a {0}", "test");
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(message.ToString(), result.FullMessage);
            Assert.AreEqual(message.ToString(), result.ShortMessage);
        }

        [Test]
        public void Strings()
        {
            var layout = new GelfLayout();

            var message = "test";
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(message, result.FullMessage);
            Assert.AreEqual(message, result.ShortMessage);
        }

        [Test]
        public void CustomObjectAddedAsAdditionalProperties()
        {
            var layout = new GelfLayout();

            var message = new { Test = 1, Test2 = "YES!!!" };
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_Test"], message.Test.ToString());
            Assert.AreEqual(result["_Test2"], message.Test2);
        }

        [Test]
        public void ThreadContextPropertiesAddedAsAdditionalProperties()
        {
            var layout = new GelfLayout();

            var message = "test";

            ThreadContext.Properties["TraceID"] = 1;

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message);
            Assert.AreEqual(result["_TraceID"], "1");
        }

        [Test]
        public void BaseGelfDataIncluded()
        {
            var layout = new GelfLayout();
            var message = "test";
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message);
            Assert.AreEqual(result.Facility, "Gelf");
            Assert.AreEqual(result.Host, Environment.MachineName);
            Assert.AreEqual(result.Level, (int)LocalSyslogAppender.SyslogSeverity.Debug);
            Assert.IsTrue(result.TimeStamp >= DateTime.Now.AddMinutes(-1));
            Assert.AreEqual(result.Version, "1.0");
        }

        [Test]
        public void CustomPropertyWithUnderscoreIsAddedCorrectly()
        {
            var layout = new GelfLayout();

            var message = "test";

            ThreadContext.Properties["_TraceID"] = 1;

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message);
            Assert.AreEqual(result["_TraceID"], "1");
        }

        [Test]
        public void CustomObjectWithMessageProperty()
        {
            var layout = new GelfLayout();
            var message = new { Message = "Success", Test = 1 };

            var loggingEvent = GetLogginEvent(message);
            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message.Message);
            Assert.AreEqual(result["_Test"], message.Test.ToString());

            var message2 = new { FullMessage = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message2);
            result = GetMessage(layout, loggingEvent);


            Assert.AreEqual(result.FullMessage, message2.FullMessage);
            Assert.AreEqual(result["_Test"], message2.Test.ToString());

            var message3 = new { message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message3);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message3.message);
            Assert.AreEqual(result["_Test"], message3.Test.ToString());

            var message4 = new { full_Message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message4);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message4.full_Message);
            Assert.AreEqual(result["_Test"], message4.Test.ToString());
        }

        [Test]
        public void CustomObjectWithShortMessageProperty()
        {
            var layout = new GelfLayout();
            var message = new { ShortMessage = "Success", Test = 1 };

            var loggingEvent = GetLogginEvent(message);
            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.ShortMessage, message.ShortMessage);
            Assert.AreEqual(result["_Test"], message.Test.ToString());

            var message2 = new { short_message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message2);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.ShortMessage, message2.short_message);
            Assert.AreEqual(result["_Test"], message2.Test.ToString());

            var message3 = new { message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message3);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message3.message);
            Assert.AreEqual(result.ShortMessage, message3.message);
            Assert.AreEqual(result["_Test"], message3.Test.ToString());

            var message4 = new { message = "Success", short_message = "test", Test = 1 };
            loggingEvent = GetLogginEvent(message4);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message4.message);
            Assert.AreEqual(result.ShortMessage, message4.short_message);
            Assert.AreEqual(result["_Test"], message4.Test.ToString());
        }

        [Test]
        public void DictionaryMessage()
        {
            var layout = new GelfLayout();

            var message = new Dictionary<string, string> { { "Test", "1" }, { "_Test2", "2" } };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_Test"], message["Test"]);
            Assert.AreEqual(result["_Test2"], message["_Test2"]);
        }
        
        [Test]
        public void ToStringOnObjectIfNoMessageIsProvided()
        {
            var layout = new GelfLayout();

            var message = new { Test = 1, Test2 = 2 };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_Test"], message.Test.ToString());
            Assert.AreEqual(result["_Test2"], message.Test2.ToString());
            Assert.AreEqual(result.FullMessage, message.ToString());
            Assert.AreEqual(result.ShortMessage, message.ToString().TruncateMessage(250));
        }

        [Test]
        public void IncludeLocationInformation()
        {
            var layout = new GelfLayout();
            layout.IncludeLocationInformation = true;

            var message = "test";
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(message, result.FullMessage);
            Assert.AreEqual(message, result.ShortMessage);
            Assert.IsNotNullOrEmpty(result.Line);
            Assert.IsNotNullOrEmpty(result.File);
        }

        private GelfMessage GetMessage(GelfLayout layout, LoggingEvent message)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
                layout.Format(sw, message);

            return JsonConvert.DeserializeObject<GelfMessage>(sb.ToString());
        }
        private static LoggingEvent GetLogginEvent(object message)
        {
            return new LoggingEvent((Type)null, (ILoggerRepository)null, null, Level.Debug, message, null);
        }
    }
}
