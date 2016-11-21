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
        public event EventHandler<ExecuteExceptionEventArgs> Exceptions;

        public event EventHandler<ReceivedEventArgs> Received;

        public Listener(IConnectionFactory factory, string ns)
            : base(factory, ns)
        {
        }

        public void SubscribeEvents<TMessage>(IContainer container)
        {
            var queue = EventQueueDeclare<TMessage>();
            ConfigureConsumer<TMessage>(container, queue);
        }


        public void SubscribeCommands<TMessage>(IContainer container)
        {
            var queue = CommandQueueDeclare<TMessage>();
            ConfigureConsumer<TMessage>(container, queue);
        }

        private string EventQueueDeclare<TMessage>()
        {
            var queue = Channel.QueueDeclare().QueueName;
            Channel.QueueBind(queue: queue,
                          exchange: Namespace,
                          routingKey: TopicFor<TMessage>());

            return queue;
        }

        private void ConfigureConsumer<TMessage>(IContainer container, string queue)
        {
            var receiverType = ResolveReceiverType<TMessage>(container);

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (o, ea) =>
            {
                var json = ea.Body.ToContent();
                Received?.Invoke(this, new ReceivedEventArgs
                {
                    MessageType = typeof(TMessage),
                    HandledBy = receiverType,
                    Message = json
                });

                try
                {
                    Handle(container, json.ToObject<TMessage>());
                    Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Exceptions?.Invoke(this, new ExecuteExceptionEventArgs
                    {
                        Receiver = receiverType,
                        Exception = ex
                    });
                }
            };

            Channel.BasicConsume(queue: queue,
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
