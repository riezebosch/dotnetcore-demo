using FirstThen;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabbitmq_demo
{
    public class Receiver : IDisposable
    {
        private readonly string _hostname;
        private readonly string _exchange;

        IConnection connection;
        IModel channel;

        public Receiver(string hostname = "localhost", string exchange = "demo")
        {
            _hostname = hostname;
            _exchange = exchange;

            IConnectionFactory factory = new ConnectionFactory() { HostName = _hostname };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
        }

        public void Dispose()
        {
            connection.Dispose();
            channel.Dispose();
        }

        public IInvocant<T> Subscribe<T>()
        {
            var routingkey = typeof(T).FullName;
            channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout);

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName,
                              exchange: _exchange,
                              routingKey: routingkey);

            var invocant = new Invocant<T>();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) => invocant.Invoke(Convert<T>(ea.Body));

            channel.BasicConsume(queue: queueName,
                             noAck: true,
                             consumer: consumer);

            return invocant;
        }

        private static T Convert<T>(byte[] body)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body));
        }
    }
}
