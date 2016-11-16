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
    public class ChannelBase : IDisposable
    {
        protected IConnection Connection;
        protected IModel Channel { get; private set; }
        protected string Exchange { get; }
        protected virtual IDictionary<string, object> Arguments { get; } = null;

        public ChannelBase(IConnectionFactory factory, string exchange)
        {
            Exchange = exchange;
            Connection = factory.CreateConnection();

            Channel = Connection.CreateModel();
            Channel.ExchangeDeclare(exchange: Exchange,
                type: ExchangeType.Topic);
       }

        protected IModel CommandQueueDeclare(string routingKey)
        {
            Channel.QueueDeclare(queue: routingKey,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: Arguments);

            return Channel;
        }


        public virtual void Dispose()
        {
            Channel.Dispose();
            Connection.Dispose();
        }
    }
}
