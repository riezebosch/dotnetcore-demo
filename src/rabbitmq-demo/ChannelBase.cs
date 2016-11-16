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
        protected string Namespace { get; }
        protected virtual IDictionary<string, object> Arguments { get; } = null;

        public ChannelBase(IConnectionFactory factory, string ns)
        {
            Namespace = ns;
            Connection = factory.CreateConnection();

            Channel = Connection.CreateModel();
            Channel.ExchangeDeclare(exchange: Namespace,
                type: ExchangeType.Topic);
       }

        protected IModel CommandQueueDeclare(string queue)
        {
            Channel.QueueDeclare(queue: queue,
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
