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
    public class Listener : ChannelBase
    {
        public event EventHandler<ReceivedEventArgs> Received;

        public Listener(IConnectionFactory factory, string exchange)
            : base(factory, exchange)
        {
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
            var receiverType = ResolveReceiverType<TContract>(container);

            var routingKey = typeof(TContract).Name;
            var queueName = Channel.QueueDeclare().QueueName;
            Channel.QueueBind(queue: queueName,
                          exchange: Exchange,
                          routingKey: routingKey);

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (o, e) =>
            {
                var json = e.Body.ToContent();
                Received?.Invoke(this, new ReceivedEventArgs
                {
                    Topic = routingKey,
                    HandledBy = receiverType,
                    Message = json
                });

                Handle(container, json.ToObject<TContract>());
            };

            Channel.BasicConsume(queue: queueName,
                 noAck: true,
                 consumer: consumer);
        }

        public void Handle<TContract>(IContainer container,
            TContract message)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var receiver = scope.Resolve<IReceive<TContract>>();
                receiver.Execute(message);
            }
        }

        private static Type ResolveReceiverType<TContract>(IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var receiver = scope.Resolve<IReceive<TContract>>();
                return receiver.GetType();
            }
        }
    }
}
