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

        public Listener(IConnectionFactory factory, string ns)
            : base(factory, ns)
        {
        }

        public void SubscribeEvents<TMessage>(IContainer container)
        {
            var receiverType = ResolveReceiverType<TMessage>(container);

            var topic = typeof(TMessage).Name;
            var queueName = Channel.QueueDeclare().QueueName;
            Channel.QueueBind(queue: queueName,
                          exchange: Namespace,
                          routingKey: topic);

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (o, e) =>
            {
                var json = e.Body.ToContent();
                Received?.Invoke(this, new ReceivedEventArgs
                {
                    Topic = topic,
                    HandledBy = receiverType,
                    Message = json
                });

                Handle(container, json.ToObject<TMessage>());
            };

            Channel.BasicConsume(queue: queueName,
                 noAck: false,
                 consumer: consumer);
        }


        public void SubscribeCommands<TMessage>(IContainer container)
        {
            var receiverType = ResolveReceiverType<TMessage>(container);

            var topic = typeof(TMessage).Name;
            var routingKey = $"{Namespace}.{topic}";

            var channel = CommandQueueDeclare(routingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (o, ea) =>
            {
                var json = ea.Body.ToContent();
                Received?.Invoke(this, new ReceivedEventArgs
                {
                    Topic = topic,
                    HandledBy = receiverType,
                    Message = json
                });

                Handle(container, json.ToObject<TMessage>());
                Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: routingKey,
                 noAck: false,
                 consumer: consumer);
        }

        public void Handle<TMessage>(IContainer container,
            TMessage message)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var receiver = scope.Resolve<IReceive<TMessage>>();
                receiver.Execute(message);
            }
        }

        private static Type ResolveReceiverType<TMessage>(IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var receiver = scope.Resolve<IReceive<TMessage>>();
                return receiver.GetType();
            }
        }
    }
}
