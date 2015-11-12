using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using log4net.Core;

namespace gelf4net.Appender
{
    public class AsyncGelfUdpAppender : GelfUdpAppender
     {
        
        private readonly ConcurrentQueue<LoggingEvent> _pendingTasks;
        private readonly ManualResetEvent _manualResetEvent;
        private bool _onClosing;

        public AsyncGelfUdpAppender() 
        {
            _pendingTasks = new ConcurrentQueue<LoggingEvent>();
            _manualResetEvent = new ManualResetEvent(false);
            Start();
        }
        protected override void Append(LoggingEvent[] loggingEvents)
        {
            foreach (var loggingEvent in loggingEvents)
                Append(loggingEvent);
        }
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (FilterEvent(loggingEvent))
                _pendingTasks.Enqueue(loggingEvent);
        }
        private void Start()
        {
            if (_onClosing)return;
            var thread = new Thread(LogMessages);
            thread.Start();
        }
        private void LogMessages()
        {
            while (!_onClosing)
            {
                LoggingEvent loggingEvent;
                while (!_pendingTasks.TryDequeue(out loggingEvent))
                {
                    Thread.Sleep(10);
                    if (_onClosing)
                    {
                        try
                        {
                            base.Append(_pendingTasks.ToArray());
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            break;
                        }
                    }
                }
                if (loggingEvent != null)
                {
                    base.Append(loggingEvent);
                }
            }
            _manualResetEvent.Set();
        }
        protected override void OnClose()
        {
            Debug.WriteLine("Closing Async Appender");
            _onClosing = true;
            _manualResetEvent.WaitOne(TimeSpan.FromSeconds(10));
            Debug.WriteLine("Logging thread has stopped");
            base.OnClose();
        }
    }
}