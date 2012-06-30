using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Esilog.Gelf4net.Appender;

namespace Gelf4netTest
{
    class TestGelf4NetAppenderWrapper : Gelf4NetAppender
    {
        public void TestAppend(log4net.Core.LoggingEvent loggingEvent)
        {
            Append(loggingEvent);
        }
    }
}
