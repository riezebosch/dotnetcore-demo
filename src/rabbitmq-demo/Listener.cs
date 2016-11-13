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
            ValidateReceiverRegistration<TContract>(container);

            var routingkey = typeof(TContract).Name;
            var queueName = Channel.QueueDeclare().QueueName;
            Channel.QueueBind(queue: queueName,
                          exchange: Exchange,
                          routingKey: routingkey);

            var handler = new MessageReceivedHandler<TContract>(
                container,
                (c, m) => Received?.Invoke(this, new ReceivedEventArgs
                {
                    HandledBy = c,
                    Topic = routingkey,
                    Message = m
                }));

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += handler.Handle;

            Channel.BasicConsume(queue: queueName,
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
