using RabbitMQ.Client;

namespace Esilog.Gelf4net.Transport
{
    class AmqpTransport : GelfTransport
    {
        public string VirtualHost { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Queue { get; set; }

        public override void Send(string serverHostName, string serverIpAddress, int serverPort, string message)
        {
            //Create the Connection 
            var factory = new ConnectionFactory()
            {
                Protocol = Protocols.FromEnvironment(),
                HostName = serverIpAddress,
                Port = serverPort,
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
