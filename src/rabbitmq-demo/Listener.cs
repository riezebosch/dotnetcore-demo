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
            var routingkey = typeof(TContract).Name;
            using (var scope = container.BeginLifetimeScope())
            {
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                              exchange: _exchange,
                              routingKey: routingkey);

                var handler = new MessageReceivedHandle<TContract>(container);
                handler.Received += Received;

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += handler.Handle;


                channel.BasicConsume(queue: queueName,
                                 noAck: true,
                                 consumer: consumer);
            }
        }

        internal class MessageReceivedHandle<TContract>
        {
            private readonly IContainer container;

            public event EventHandler<ReceivedEventArgs> Received;

            public MessageReceivedHandle(IContainer container)
            {
                this.container = container;
            }

            public void Handle(object sender, BasicDeliverEventArgs ea)
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var receiver = scope.Resolve<IReceive<TContract>>();
                    var content = Encoding.UTF8.GetString(ea.Body);

                    Received?.Invoke(this, new ReceivedEventArgs
                    {
                        HandledBy = receiver.GetType(),
                        Topic = typeof(TContract).Name,
                        Message = content
                    });

                    var item = JsonConvert.DeserializeObject<TContract>(content);
                    receiver.Execute(item);
                }
            }
        }
    }
}
