using log4net.Appender;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gelf4Net.Appender
{
    public class GelfAmqpAppender : AppenderSkeleton
    {
        public GelfAmqpAppender()
        {
            Encoding = Encoding.UTF8;
        }

        protected ConnectionFactory ConnectionFactory { get; set; }
        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public string Exchange { get; set; }
        public string Key { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int RetryAfterMilliseconds { get; set; } = 5000;
        public bool UseTls { get; set; } = true;
        public Encoding Encoding { get; set; }
        protected IConnection Connection { get; set; }
        protected IModel Channel { get; set; }
        private static volatile object _syncLock = new object();
        private TimeSpan _bootTimeoutTimeSpan;
        private bool _shouldWaitBootTimeout = false;
        private bool _isWatingBootTimeout = false;


        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _bootTimeoutTimeSpan = TimeSpan.FromMilliseconds(RetryAfterMilliseconds);

            InitializeConnectionFactory();
        }

        protected virtual void InitializeConnectionFactory()
        {
            ConnectionFactory = new ConnectionFactory()
            {
                HostName = RemoteAddress,
                Port = RemotePort,
                VirtualHost = VirtualHost,
                UserName = Username,
                Password = Password,
                AutomaticRecoveryEnabled = true,
                Ssl = new SslOption
                {
                    Enabled = UseTls,
                    ServerName = RemoteAddress,
                    Version = SslProtocols.None
                };
            };
            Connection = ConnectionFactory.CreateConnection();
            Channel = Connection.CreateModel();
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            var message = RenderLoggingEvent(loggingEvent).GzipMessage(Encoding);
            SendMessage(message);
        }

        protected void SendMessage(byte[][] loggingEvents)
        {
            foreach (var loggingEvent in loggingEvents)
            {
                SendMessage(loggingEvent);
            }
        }

        protected Task<bool> SendMessageAsync(string logMessage)
        {
            try
            {
                SendMessage(logMessage.GzipMessage(Encoding));
                _shouldWaitBootTimeout = false;
                return Task.FromResult(true);
            }
            catch (System.Exception e)
            {
                _shouldWaitBootTimeout = true;
                ErrorHandler.Error("Unable to send logging event", e);
                return Task.FromResult(false);
            }
        }

        protected void SendMessage(byte[] payload)
        {
            if (WaitForConnectionToConnectOrReconnect(new TimeSpan(0, 0, 0, 0, 500)))
            {
                lock (_syncLock)
                    Channel.BasicPublish(Exchange, Key, null, payload);
            }
        }

        private bool WaitForConnectionToConnectOrReconnect(TimeSpan timeToWait)
        {
            if (Connection != null && Connection.IsOpen)
            {
                return true;
            }
            var dt = DateTime.Now;
            while (Connection != null && !Connection.IsOpen && (DateTime.Now - dt) < timeToWait)
            {
                Thread.Sleep(1);
            }
            if (Connection != null)
            {
                return Connection.IsOpen;
            }
            return false;
        }

        protected override void OnClose()
        {
            if (Channel != null)
            {
                Channel.Close();
                Channel.Dispose();
            }
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }

        protected async Task WaitToSendOnConnectionAsync()
        {
            if (_shouldWaitBootTimeout)
            {
                _isWatingBootTimeout = true;
                await Task.Delay(_bootTimeoutTimeSpan);
                _shouldWaitBootTimeout = false;
                _isWatingBootTimeout = false;
            }
        }

        protected bool IsWaiting() => _isWatingBootTimeout;

#if NETSTANDARD1_5
        private static Task CompletedTask = Task.CompletedTask;
#else
        private static Task CompletedTask = Task.FromResult<bool>(true);
#endif

    }
}
