﻿using Gelf4Net.Layout;
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
using System.IO;
using System.Text;
using NUnit.Framework.Legacy;

namespace Gelf4Net.Tests.Layout
{
    [TestFixture]
    [Obsolete("tests obsolete code")]
    public class GelfLayoutTests
    {
        [Test]
        public void StringFormat()
        {
            var layout = new GelfLayout();

            var message = new SystemStringFormat(CultureInfo.CurrentCulture, "This is a {0}", "test");
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(message.ToString(), result.FullMessage);
            ClassicAssert.AreEqual(message.ToString(), result.ShortMessage);
        }

        [Test]
        public void Strings()
        {
            var layout = new GelfLayout();

            var message = "test";
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(message, result.FullMessage);
            ClassicAssert.AreEqual(message, result.ShortMessage);
        }

        [Test]
        public void CustomObjectAddedAsAdditionalProperties()
        {
            var layout = new GelfLayout();

            var message = new { Test = 1, Test2 = "YES!!!" };
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Test"], message.Test);
            ClassicAssert.AreEqual(result["_Test2"], message.Test2);
        }

        [Test]
        public void ThreadContextPropertiesAddedAsAdditionalProperties()
        {
            var layout = new GelfLayout();

            var message = "test";

            ThreadContext.Properties["TraceID"] = 1;

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message);
            ClassicAssert.AreEqual(result["_TraceID"], 1);
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

            ClassicAssert.AreEqual(result["_obj"], someObject.ToString());
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

            ClassicAssert.AreEqual(result["_decimal"], 1);
            ClassicAssert.AreEqual(result["_double"], 1);
            ClassicAssert.AreEqual(result["_float"], 1);
            ClassicAssert.AreEqual(result["_int"], 1);
            ClassicAssert.AreEqual(result["_uint"], 1);
            ClassicAssert.AreEqual(result["_long"], 1);
            ClassicAssert.AreEqual(result["_ulong"], 1);
            ClassicAssert.AreEqual(result["_short"], 1);
            ClassicAssert.AreEqual(result["_ushort"], 1);
            ClassicAssert.AreEqual(result["_nullable"], 1);
        }

        [Test]
        public void BaseGelfDataIncluded()
        {
            var layout = new GelfLayout();
            var message = "test";
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message);
            ClassicAssert.AreEqual(result.Facility, "Gelf");
            ClassicAssert.AreEqual(result.Host, Environment.MachineName);
            ClassicAssert.AreEqual(result.Level, (int)LocalSyslogAppender.SyslogSeverity.Debug);
            ClassicAssert.IsTrue(result.TimeStamp >= DateTime.Now.AddMinutes(-1));
            ClassicAssert.AreEqual(result.Version, "1.0");
        }

        [Test]
        public void CustomPropertyWithUnderscoreIsAddedCorrectly()
        {
            var layout = new GelfLayout();

            var message = "test";

            ThreadContext.Properties["_TraceID"] = 1;

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message);
            ClassicAssert.AreEqual(result["_TraceID"], 1);
        }

        [Test]
        public void CustomObjectWithMessageProperty()
        {
            var layout = new GelfLayout();
            var message = new { Message = "Success", Test = 1 };

            var loggingEvent = GetLogginEvent(message);
            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message.Message);
            ClassicAssert.AreEqual(result["_Test"], message.Test);

            var message2 = new { FullMessage = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message2);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message2.FullMessage);
            ClassicAssert.AreEqual(result["_Test"], message2.Test);

            var message3 = new { message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message3);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message3.message);
            ClassicAssert.AreEqual(result["_Test"], message3.Test);

            var message4 = new { full_Message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message4);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message4.full_Message);
            ClassicAssert.AreEqual(result["_Test"], message4.Test);
        }

        [Test]
        public void CustomObjectWithShortMessageProperty()
        {
            var layout = new GelfLayout();
            var message = new { ShortMessage = "Success", Test = 1 };

            var loggingEvent = GetLogginEvent(message);
            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.ShortMessage, message.ShortMessage);
            ClassicAssert.AreEqual(result["_Test"], message.Test);

            var message2 = new { short_message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message2);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.ShortMessage, message2.short_message);
            ClassicAssert.AreEqual(result["_Test"], message2.Test);

            var message3 = new { message = "Success", Test = 1 };
            loggingEvent = GetLogginEvent(message3);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message3.message);
            ClassicAssert.AreEqual(result.ShortMessage, message3.message);
            ClassicAssert.AreEqual(result["_Test"], message3.Test);

            var message4 = new { message = "Success", short_message = "test", Test = 1 };
            loggingEvent = GetLogginEvent(message4);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message4.message);
            ClassicAssert.AreEqual(result.ShortMessage, message4.short_message);
            ClassicAssert.AreEqual(result["_Test"], message4.Test);
        }

        [Test]
        public void DictionaryMessage()
        {
            var layout = new GelfLayout();
            var message = new Dictionary<string, string> { { "Test", "1" }, { "_Test2", "2" } };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Test"], message["Test"]);
            ClassicAssert.AreEqual(result["_Test2"], message["_Test2"]);
        }

        [Test]
        public void PatternConversionInAdditionalProperties()
        {
            var layout = new GelfLayout();
            layout.AdditionalFields = "Level:%level,AppDomain:%a,LoggerName:%c{1},ThreadName:%t";
            var message = new { Message = "Test" };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Level"], "DEBUG");
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            ClassicAssert.AreEqual("Class", result["_LoggerName"]);
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

            ClassicAssert.AreEqual(result["_Level"], "DEBUG");
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            ClassicAssert.AreEqual("Class", result["_LoggerName"]);
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

            ClassicAssert.AreEqual(result["_Level"], "DEBUG");
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            ClassicAssert.AreEqual("Class", result["_LoggerName"]);
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

            ClassicAssert.AreEqual(result["_Level"], "DEBUG");
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            ClassicAssert.AreEqual("Class", result["_LoggerName"]);
        }

        [Test]
        public void PatternConversionLayoutSpecified()
        {
            var layout = new GelfLayout();
            layout.ConversionPattern = "[%level] - [%c{1}]";
            var message = new { Message = "Test" };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual("[DEBUG] - [Class]", result["full_message"]);
            ClassicAssert.AreEqual("[DEBUG] - [Class]", result["short_message"]);
        }

        [Test]
        public void ToStringOnObjectIfNoMessageIsProvided()
        {
            var layout = new GelfLayout();

            var message = new { Test = 1, Test2 = 2 };

            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Test"], message.Test);
            ClassicAssert.AreEqual(result["_Test2"], message.Test2);
            ClassicAssert.AreEqual(result.FullMessage, message.ToString());
            ClassicAssert.AreEqual(result.ShortMessage, message.ToString().TruncateMessage(250));
        }

        [Test]
        public void IncludeLocationInformationWithoutLine()
        {
            var layout = new GelfLayout();
            layout.IncludeLocationInformation = true;

            var message = "test";
            var loggingEvent = GetLogginEvent(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(message, result.FullMessage);
            ClassicAssert.AreEqual(message, result.ShortMessage);
            ClassicAssert.IsEmpty(result.Line);
            ClassicAssert.IsEmpty(result.File);
        }

        [Test]
        public void NullPropertyValueDoesNotCauseException()
        {
            var layout = new GelfLayout();
            layout.IncludeLocationInformation = true;

            var message = "test";
            var loggingEvent = GetLogginEvent(message);
            loggingEvent.Properties["nullProperty"] = null;

            ClassicAssert.DoesNotThrow(() => GetMessage(layout, loggingEvent));
        }

        private GelfMessage GetMessage(GelfLayout layout, LoggingEvent message, bool sendTimeStampAsString = true)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
                layout.Format(sw, message);

            if (sendTimeStampAsString)
            {
                var gelfMessage = new GelfMessage(true);
                JsonConvert.PopulateObject(sb.ToString(), gelfMessage);
                return gelfMessage;
            }
            
            return JsonConvert.DeserializeObject<GelfMessage>(sb.ToString());
        }

        [Test]
        public void StringFormat_WithRenderedMessage()
        {
            var layout = new GelfLayout();

            var message = new SystemStringFormat(CultureInfo.CurrentCulture, "This is a {0}", "test");
            var loggingEvent = GetLogginEventRenderedMessage(message.ToString());

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(message.ToString(), result.FullMessage);
            ClassicAssert.AreEqual(message.ToString(), result.ShortMessage);
        }

        [Test]
        public void Strings_WithRenderedMessage()
        {
            var layout = new GelfLayout();

            var message = "test";
            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(message, result.FullMessage);
            ClassicAssert.AreEqual(message, result.ShortMessage);
        }

        [Test]
        public void CustomObjectAddedAsAdditionalProperties_WithRenderedMessage()
        {
            var layout = new GelfLayout();

            var message = new { Test = 1, Test2 = "YES!!!" };
            var messageJson = JsonConvert.SerializeObject(message);
            var loggingEvent = GetLogginEventRenderedMessage(messageJson);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Test"], message.Test.ToString());
            ClassicAssert.AreEqual(result["_Test2"], message.Test2.ToString());
        }

        [Test]
        public void ThreadContextPropertiesAddedAsAdditionalProperties_WithRenderedMessage()
        {
            var layout = new GelfLayout();

            var message = "test";

            ThreadContext.Properties["TraceID"] = 1;

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message);
            ClassicAssert.AreEqual(result["_TraceID"], 1);
        }

        [Test]
        public void ObjectPropertiesConvertedToStrings_WithRenderedMessage()
        {
            var layout = new GelfLayout();

            var message = "test";

            var someObject = new object();
            ThreadContext.Properties["obj"] = someObject;

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_obj"], someObject.ToString());
        }

        [Test]
        public void NumericPropertiesAreNotConvertedToStrings_WithRenderedMessage()
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

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_decimal"], 1);
            ClassicAssert.AreEqual(result["_double"], 1);
            ClassicAssert.AreEqual(result["_float"], 1);
            ClassicAssert.AreEqual(result["_int"], 1);
            ClassicAssert.AreEqual(result["_uint"], 1);
            ClassicAssert.AreEqual(result["_long"], 1);
            ClassicAssert.AreEqual(result["_ulong"], 1);
            ClassicAssert.AreEqual(result["_short"], 1);
            ClassicAssert.AreEqual(result["_ushort"], 1);
            ClassicAssert.AreEqual(result["_nullable"], 1);
        }

        [Test]
        public void BaseGelfDataIncluded_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            var message = "test";
            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message);
            ClassicAssert.AreEqual(result.Facility, "Gelf");
            ClassicAssert.AreEqual(result.Host, Environment.MachineName);
            ClassicAssert.AreEqual(result.Level, (int)LocalSyslogAppender.SyslogSeverity.Debug);
            ClassicAssert.IsTrue(result.TimeStamp >= DateTime.Now.AddMinutes(-1));
            ClassicAssert.AreEqual(result.Version, "1.0");
        }

        [Test]
        public void CustomPropertyWithUnderscoreIsAddedCorrectly_WithRenderedMessage()
        {
            var layout = new GelfLayout();

            var message = "test";

            ThreadContext.Properties["_TraceID"] = 1;

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message);
            ClassicAssert.AreEqual(result["_TraceID"], 1);
        }

        [Test]
        public void CustomObjectWithMessageProperty_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            var message = new { Message = "Success", Test = 1 };
            var messageJson = JsonConvert.SerializeObject(message);

            var loggingEvent = GetLogginEventRenderedMessage(messageJson);
            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message.Message);
            ClassicAssert.AreEqual(result["_Test"], message.Test.ToString());

            var message2 = new { FullMessage = "Success", Test = 1 };
            var message2Json = JsonConvert.SerializeObject(message2);
            loggingEvent = GetLogginEventRenderedMessage(message2Json);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message2.FullMessage);
            ClassicAssert.AreEqual(result["_Test"], message2.Test.ToString());

            var message3 = new { message = "Success", Test = 1 };
            var message3Json = JsonConvert.SerializeObject(message3);
            loggingEvent = GetLogginEventRenderedMessage(message3Json);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message3.message);
            ClassicAssert.AreEqual(result["_Test"], message3.Test.ToString());

            var message4 = new { full_Message = "Success", Test = 1 };
            var message4Json = JsonConvert.SerializeObject(message4);
            loggingEvent = GetLogginEventRenderedMessage(message4Json);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message4.full_Message);
            ClassicAssert.AreEqual(result["_Test"], message4.Test.ToString());
        }

        [Test]
        public void CustomObjectWithShortMessageProperty_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            var message = new { ShortMessage = "Success", Test = 1 };
            var messageJson = JsonConvert.SerializeObject(message);

            var loggingEvent = GetLogginEventRenderedMessage(messageJson);
            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.ShortMessage, message.ShortMessage);
            ClassicAssert.AreEqual(result["_Test"], message.Test.ToString());

            var message2 = new { short_message = "Success", Test = 1 };
            var message2Json = JsonConvert.SerializeObject(message2);
            loggingEvent = GetLogginEventRenderedMessage(message2Json);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.ShortMessage, message2.short_message);
            ClassicAssert.AreEqual(result["_Test"], message2.Test.ToString());

            var message3 = new { message = "Success", Test = 1 };
            var message3Json = JsonConvert.SerializeObject(message3);
            loggingEvent = GetLogginEventRenderedMessage(message3Json);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message3.message);
            ClassicAssert.AreEqual(result.ShortMessage, message3.message);
            ClassicAssert.AreEqual(result["_Test"], message3.Test.ToString());

            var message4 = new { message = "Success", short_message = "test", Test = 1 };
            var message4Json = JsonConvert.SerializeObject(message4);
            loggingEvent = GetLogginEventRenderedMessage(message4Json);
            result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result.FullMessage, message4.message);
            ClassicAssert.AreEqual(result.ShortMessage, message4.short_message);
            ClassicAssert.AreEqual(result["_Test"], message4.Test.ToString());
        }

        [Test]
        public void PatternConversionInAdditionalProperties_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            layout.AdditionalFields = "Level:%level,AppDomain:%a,LoggerName:%c{1},ThreadName:%t";
            var message = JsonConvert.SerializeObject(new { Message = "Test" });

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Level"], "DEBUG");
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            ClassicAssert.AreEqual("Class", result["_LoggerName"]);
        }

        [Test]
        public void PatternConversionInAdditionalPropertiesWithCustomSeparators_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            layout.FieldSeparator = "||";
            layout.KeyValueSeparator = "=>";
            layout.AdditionalFields = "Level=>%level||AppDomain=>%a||LoggerName=>%c{1}||ThreadName=>%t";
            var message = JsonConvert.SerializeObject(new { Message = "Test" });

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Level"], "DEBUG");
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            ClassicAssert.AreEqual("Class", result["_LoggerName"]);
        }

        [Test]
        public void PatternConversionInAdditionalPropertiesWithCustomKeyValueSeparator_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            layout.KeyValueSeparator = "=>";
            layout.AdditionalFields = "Level=>%level,AppDomain=>%a,LoggerName=>%c{1},ThreadName=>%t";
            var message = JsonConvert.SerializeObject(new { Message = "Test" });

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Level"], "DEBUG");
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            ClassicAssert.AreEqual("Class", result["_LoggerName"]);
        }

        [Test]
        public void PatternConversionInAdditionalPropertiesWithCustomFieldSeparator_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            layout.FieldSeparator = "||";
            layout.AdditionalFields = "Level:%level||AppDomain:%a||LoggerName:%c{1}||ThreadName:%t";
            var message = JsonConvert.SerializeObject(new { Message = "Test" });

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Level"], "DEBUG");
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_AppDomain"].ToString()));
            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(result["_ThreadName"].ToString()));
            ClassicAssert.AreEqual("Class", result["_LoggerName"]);
        }

        [Test]
        public void PatternConversionLayoutSpecified_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            layout.ConversionPattern = "[%level] - [%c{1}]";
            var message = JsonConvert.SerializeObject(new { Message = "Test" });

            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual("[DEBUG] - [Class]", result["full_message"]);
            ClassicAssert.AreEqual("[DEBUG] - [Class]", result["short_message"]);
        }

        [Test]
        public void ToStringOnObjectIfNoMessageIsProvided_WithRenderedMessage()
        {
            var layout = new GelfLayout();

            var message = new { Test = 1, Test2 = 2 };
            var messageJson = JsonConvert.SerializeObject(message);

            var loggingEvent = GetLogginEventRenderedMessage(messageJson);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(result["_Test"], message.Test.ToString());
            ClassicAssert.AreEqual(result["_Test2"], message.Test2.ToString());
            ClassicAssert.AreEqual(result.FullMessage, messageJson);
            ClassicAssert.AreEqual(result.ShortMessage, messageJson.TruncateMessage(250));
        }

        [Test]
        public void IncludeLocationInformation_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            layout.IncludeLocationInformation = true;

            var message = "test";
            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(message, result.FullMessage);
            ClassicAssert.AreEqual(message, result.ShortMessage);
            ClassicAssert.That(result.Line, Is.Not.Null.And.Not.Empty);
            ClassicAssert.That(result.File, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void NullPropertyValueDoesNotCauseException_WithRenderedMessage()
        {
            var layout = new GelfLayout();
            layout.IncludeLocationInformation = true;

            var message = "test";
            var loggingEvent = GetLogginEventRenderedMessage(message);
            loggingEvent.Properties["nullProperty"] = null;

            ClassicAssert.DoesNotThrow(() => GetMessage(layout, loggingEvent));
        }

        private static LoggingEvent GetLogginEvent(object message)
        {
            var @event = new LoggingEvent((Type)null, (ILoggerRepository)null, "Test.Logger.Class", Level.Debug, message, null);
            return @event;
        }

        [Test]
        public void SendTimeStampAsString()
        {
            var layout = new GelfLayout();
            layout.IncludeLocationInformation = true;
            layout.SendTimeStampAsString = true;

            var message = "test";
            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(message, result.FullMessage);
            ClassicAssert.AreEqual(message, result.ShortMessage);
            ClassicAssert.AreEqual(typeof(string), result["timestamp"].GetType());
            ClassicAssert.That(result.Line, Is.Not.Null.And.Not.Empty);
            ClassicAssert.That(result.File, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void SendTimeStampAsNumber()
        {
            var layout = new GelfLayout();
            layout.IncludeLocationInformation = true;
            layout.SendTimeStampAsString = false;

            var message = "test";
            var loggingEvent = GetLogginEventRenderedMessage(message);

            var result = GetMessage(layout, loggingEvent);

            ClassicAssert.AreEqual(message, result.FullMessage);
            ClassicAssert.AreEqual(message, result.ShortMessage);
            ClassicAssert.AreEqual(typeof(double), result["timestamp"].GetType());
            ClassicAssert.That(result.Line, Is.Not.Null.And.Not.Empty);
            ClassicAssert.That(result.File, Is.Not.Null.And.Not.Empty);
        }

        private static LoggingEvent GetLogginEventRenderedMessage(string message)
        {
            var loggingEventData = new LoggingEventData
            {
                Message = message,
                LoggerName = "Test.Logger.Class",
                Level = Level.Debug,
                TimeStampUtc = DateTime.UtcNow,
                LocationInfo = new LocationInfo("Test.Logger.Class", "Method", "Hola.cs", "8")
            };
            return new LoggingEvent(loggingEventData);
        }
    }
}
