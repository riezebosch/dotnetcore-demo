using Autofac;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public void Subscribe<TContract>(Func<IReceive<TContract>> factory)
        {
            var builder = new ContainerBuilder();
            builder.Register(c => factory());

            Subscribe<TContract>(builder.Build());
        }

        public void Subscribe<TContract>(IReceive<TContract> receiver)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(receiver);

            Subscribe<TContract>(builder.Build());
        }

        public void Subscribe<TContract>(IContainer container)
        {
            ValidateReceiverRegistration<TContract>(container);

            var routingkey = typeof(TContract).Name;
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName,
                          exchange: _exchange,
                          routingKey: routingkey);

            var handler = new MessageReceivedHandler<TContract>(
                container,
                (c, m) => Received?.Invoke(this, new ReceivedEventArgs
                {
                    HandledBy = c,
                    Topic = routingkey,
                    Message = m
                }));

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += handler.Handle;

            channel.BasicConsume(queue: queueName,
                             noAck: true,
                             consumer: consumer);

        }

        private static void ValidateReceiverRegistration<TContract>(IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                scope.Resolve<IReceive<TContract>>();
            }
        }
    }
}
