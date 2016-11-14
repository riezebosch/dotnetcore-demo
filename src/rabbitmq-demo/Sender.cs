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

        public Sender(IConnectionFactory factory, string exchange)
            : base(factory, exchange)
        {
        }

        public void Publish<T>(T input)
        {
            var routingKey = typeof(T).Name;

            var message = input.ToMessage();
            Send?.Invoke(this, new SendEventArgs { Topic = routingKey, Message = message });

            Channel.BasicPublish(exchange: Exchange,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: message.ToBody());
        }

        public void Command<T>(T input)
        {
            var routingKey = typeof(T).Name;
            var message = input.ToMessage();

            var Channel = Connection.CreateModel();
            Channel.QueueDeclare(queue: routingKey,
                autoDelete: false,
                exclusive: false);

            Channel.BasicPublish(exchange: "",
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: message.ToBody());
        }
    }
}
