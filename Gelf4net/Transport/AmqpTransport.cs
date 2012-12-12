using RabbitMQ.Client;

namespace Esilog.Gelf4net.Transport
{
    class AmqpTransport : GelfTransport
    {
        private string _hostName;
        private string _remoteIpAddress;
        private int _remotePort;

        public AmqpTransport(string hostName, string serverIpAddress, int serverPort)
        {
            _hostName = hostName;
            _remoteIpAddress = serverIpAddress;
            _remotePort = serverPort;
        }

        public string VirtualHost { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Queue { get; set; }

        public override void Send(string message)
        {
            //Create the Connection 
            var factory = new ConnectionFactory()
            {
                Protocol = Protocols.FromEnvironment(),
                HostName = _remoteIpAddress,
                Port = _remotePort,
                VirtualHost = VirtualHost,
                UserName = User,
                Password = Password
            };

            using (IConnection conn = factory.CreateConnection())
            {
                var model = conn.CreateModel();
                model.ExchangeDeclare("sendExchange", ExchangeType.Direct);
                model.QueueDeclare(Queue, true, true, true, null);
                model.QueueBind(Queue, "sendExchange", "key");
                byte[] messageBodyBytes = GzipMessage(message);
                model.BasicPublish(Queue, "key", null, messageBodyBytes);
            }
        }
    }
}
