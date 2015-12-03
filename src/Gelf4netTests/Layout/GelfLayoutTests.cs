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

            Assert.AreEqual(result["_Test"], message.Test);
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
            Assert.AreEqual(result["_TraceID"], 1);
        }

        [Test]
        public void ObjectPropertiesConvertedToStrings()
        {
            var layout = new GelfLayout();

            var message = "test";

            var someObject = new object();
            ThreadContext.Properties["obj"] = someObject;

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_obj"], someObject.ToString());
        }

        [Test]
        public void NumericPropertiesAreNotConvertedToStrings()
        {
            var layout = new GelfLayout();

            var message = "test";

            ThreadContext.Properties["decimal"] = 1m;
            ThreadContext.Properties["double"] = 1d;
            ThreadContext.Properties["float"] = 1f;
            ThreadContext.Properties["int"] = 1;
            ThreadContext.Properties["uint"] = (uint)1;
            ThreadContext.Properties["long"] = 1L;
            ThreadContext.Properties["ulong"] = 1ul;
            ThreadContext.Properties["short"] = (short)1;
            ThreadContext.Properties["ushort"] = (ushort)1;
            ThreadContext.Properties["nullable"] = (int?)1;

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_decimal"], 1);
            Assert.AreEqual(result["_double"], 1);
            Assert.AreEqual(result["_float"], 1);
            Assert.AreEqual(result["_int"], 1);
            Assert.AreEqual(result["_uint"], 1);
            Assert.AreEqual(result["_long"], 1);
            Assert.AreEqual(result["_ulong"], 1);
            Assert.AreEqual(result["_short"], 1);
            Assert.AreEqual(result["_ushort"], 1);
            Assert.AreEqual(result["_nullable"], 1);
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
            Assert.AreEqual(result["_TraceID"], 1);
        }

        [Test]
        public void CustomObjectWithMessageProperty()
        {
            var layout = new GelfLayout();
            var message = new { Message = "Success", Test = 1 };

            var loggingEvent = GetLogginEvent(message);
            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message.Message);
            Assert.AreEqual(result["_Test"], message.Test);

            var message2 = new { FullMessage = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message2);
            result = GetMessage(layout, loggingEvent);


            Assert.AreEqual(result.FullMessage, message2.FullMessage);
            Assert.AreEqual(result["_Test"], message2.Test);

            var message3 = new { message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message3);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message3.message);
            Assert.AreEqual(result["_Test"], message3.Test);

            var message4 = new { full_Message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message4);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message4.full_Message);
            Assert.AreEqual(result["_Test"], message4.Test);
        }

        [Test]
        public void CustomObjectWithShortMessageProperty()
        {
            var layout = new GelfLayout();
            var message = new { ShortMessage = "Success", Test = 1 };

            var loggingEvent = GetLogginEvent(message);
            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.ShortMessage, message.ShortMessage);
            Assert.AreEqual(result["_Test"], message.Test);

            var message2 = new { short_message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message2);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.ShortMessage, message2.short_message);
            Assert.AreEqual(result["_Test"], message2.Test);

            var message3 = new { message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message3);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message3.message);
            Assert.AreEqual(result.ShortMessage, message3.message);
            Assert.AreEqual(result["_Test"], message3.Test);

            var message4 = new { message = "Success", short_message = "test", Test = 1 };
            loggingEvent = GetLogginEvent(message4);
            result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result.FullMessage, message4.message);
            Assert.AreEqual(result.ShortMessage, message4.short_message);
            Assert.AreEqual(result["_Test"], message4.Test);
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
        public void PatternConversionInAdditionalProperties()
        {
            var layout = new GelfLayout();
            layout.AdditionalFields = "Level:%level,AppDomain:%a,LoggerName:%c{1},ThreadName:%t";
            var message = new { Message = "Test" };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_Level"], "DEBUG");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            Assert.AreEqual("Class", result["_LoggerName"]);
        }

        [Test]
        public void PatternConversionInAdditionalPropertiesWithCustomSeparators()
        {
            var layout = new GelfLayout();
            layout.FieldSeparator = "||";
            layout.KeyValueSeparator = "=>";
            layout.AdditionalFields = "Level=>%level||AppDomain=>%a||LoggerName=>%c{1}||ThreadName=>%t";            
            var message = new { Message = "Test" };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_Level"], "DEBUG");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            Assert.AreEqual("Class", result["_LoggerName"]);
        }


        [Test]
        public void PatternConversionInAdditionalPropertiesWithCustomKeyValueSeparator()
        {
            var layout = new GelfLayout();           
            layout.KeyValueSeparator = "=>";
            layout.AdditionalFields = "Level=>%level,AppDomain=>%a,LoggerName=>%c{1},ThreadName=>%t";
            var message = new { Message = "Test" };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_Level"], "DEBUG");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            Assert.AreEqual("Class", result["_LoggerName"]);
        }



        [Test]
        public void PatternConversionInAdditionalPropertiesWithCustomFieldSeparator()
        {
            var layout = new GelfLayout();
            layout.FieldSeparator = "||";
            layout.AdditionalFields = "Level:%level||AppDomain:%a||LoggerName:%c{1}||ThreadName:%t";
            var message = new { Message = "Test" };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_Level"], "DEBUG");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            Assert.AreEqual("Class", result["_LoggerName"]);
        }

        [Test]
        public void PatternConversionLayoutSpecified()
        {
            var layout = new GelfLayout();
            layout.ConversionPattern = "[%level] - [%c{1}]";
            var message = new { Message = "Test" };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual("[DEBUG] - [Class]", result["full_message"]);
            Assert.AreEqual("[DEBUG] - [Class]", result["short_message"]);
        }
        
        [Test]
        public void ToStringOnObjectIfNoMessageIsProvided()
        {
            var layout = new GelfLayout();

            var message = new { Test = 1, Test2 = 2 };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            Assert.AreEqual(result["_Test"], message.Test);
            Assert.AreEqual(result["_Test2"], message.Test2);
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
            Assert.IsNotNull(result.Line);
            Assert.IsNotEmpty(result.Line);
            Assert.IsNotNull(result.File);
            Assert.IsNotEmpty(result.File);
        }

        [Test]
        public void NullPropertyValueDoesNotCauseException()
        {
            var layout = new GelfLayout();
            layout.IncludeLocationInformation = true;

            var message = "test";
            var loggingEvent = GetLogginEvent(message);
            loggingEvent.Properties["nullProperty"] = null;

            Assert.DoesNotThrow(() => GetMessage(layout, loggingEvent));
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
            
            return new LoggingEvent((Type)null, (ILoggerRepository)null, "Test.Logger.Class", Level.Debug, message, null);
        }
    }
}
