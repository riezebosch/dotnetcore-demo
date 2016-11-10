using Autofac;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace rabbitmq_demo
{
    public class Listener : IDisposable
    {
        private readonly string _exchange;

        IConnection connection;
        IModel channel;

        public event EventHandler<ReceivedEventArgs> Received;

        public Listener(string exchange = "demo")
            : this(new ConnectionFactory() { HostName = "localhost" },
                  exchange)
        {
        }

        public Listener(IConnectionFactory factory, string exchange)
        {
            _exchange = exchange;

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Topic);
        }

        public void Dispose()
        {
            connection.Dispose();
            channel.Dispose();
        }

        public void Subscribe<T>(Func<IReceive<T>> factory)
        {
            var builder = new ContainerBuilder();
            builder.Register(c => factory());

            Subscribe<T>(builder.Build());
        }

        public void Subscribe<T>(IReceive<T> receiver)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(receiver);

            Subscribe<T>(builder.Build());
        }

        public void Subscribe<T>(IContainer container)
        {
            var routingkey = typeof(T).Name;

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName,
                              exchange: _exchange,
                              routingKey: routingkey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var receiver = scope.Resolve<IReceive<T>>();
                    var content = Encoding.UTF8.GetString(ea.Body);

                    Received?.Invoke(this, new ReceivedEventArgs
                    {
                        HandledBy = receiver.GetType(),
                        Topic = routingkey,
                        Message = content
                    });

                    receiver.Execute(JsonConvert.DeserializeObject<T>(content));
                }
            };

            channel.BasicConsume(queue: queueName,
                             noAck: true,
                             consumer: consumer);
        }
    }
}
