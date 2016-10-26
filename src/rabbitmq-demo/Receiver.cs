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

        public IContinuation Subscribe<T>(Action<T> action)
        {
            var routingkey = typeof(T).FullName;
            channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout);

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName,
                              exchange: _exchange,
                              routingKey: routingkey);


            var consumer = new EventingBasicConsumer(channel);

            var continuation = new Contination();
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                var item = JsonConvert.DeserializeObject<T>(message);
                action(item);

                continuation.Continue(message);
            };

            channel.BasicConsume(queue: queueName,
                             noAck: true,
                             consumer: consumer);

            return continuation;
        }

        private class Contination : IContinuation
        {
            private Action<string> _a;

            public void Continue(string message)
            {
                _a?.Invoke(message);
            }

            public void ContinueWith(Action<string> a)
            {
                _a = a;
            }
        }
    }
}
