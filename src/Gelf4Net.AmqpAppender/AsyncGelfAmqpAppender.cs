﻿using Gelf4Net.Util;
using log4net.Core;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Gelf4Net.Appender
{
    public class AsyncGelfAmqpAppender : GelfAmqpAppender
    {
        private BufferedLogSender _sender;

        public int Threads { get; set; }
        public int BufferSize { get; set; }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            var options = new BufferedSenderOptions
            {
                BufferSize = BufferSize,
                NumTasks = Threads,
            };
            _sender = new BufferedLogSender(options, SendMessageAsync, WaitToSendOnConnectionAsync, IsWaiting);
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            foreach (var loggingEvent in loggingEvents)
            {
                Append(loggingEvent);
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (FilterEvent(loggingEvent))
            {
                _sender.QueueSend(this.RenderLoggingEvent(loggingEvent));
            }
        }

        protected override void OnClose()
        {
            Debug.WriteLine("Closing Async Appender");
            _sender.Stop();
            base.OnClose();
        }
    }
}
