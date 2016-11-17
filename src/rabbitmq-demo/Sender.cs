using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabbitmq_demo
{
    public class Sender : ChannelBase, ISender
    {
        public event EventHandler<SendEventArgs> Send;

        public Sender(IConnectionFactory factory, string ns)
            : base(factory, ns)
        {
        }

        public void PublishEvent<TMessage>(TMessage input)
        {
            string topic = TopicFor<TMessage>();

            var message = input.ToMessage();
            Send?.Invoke(this, new SendEventArgs { Topic = topic, Message = message });

            Channel.BasicPublish(exchange: Namespace,
                                 routingKey: topic,
                                 basicProperties: null,
                                 body: message.ToBody());
        }

        public void PublishCommand<TMessage>(TMessage input)
        {
            var message = input.ToMessage();

            var queue = CommandQueueDeclare<TMessage>();
            Channel.BasicPublish(exchange: "",
                                 routingKey: queue,
                                 basicProperties: null,
                                 body: message.ToBody());
        }
    }
}
