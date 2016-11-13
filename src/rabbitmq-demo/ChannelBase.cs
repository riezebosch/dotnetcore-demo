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
        private IConnection _connection;
        protected IModel Channel { get; private set; }
        protected string Exchange { get; }

        public ChannelBase(IConnectionFactory factory, string exchange)
        {
            Exchange = exchange;
            _connection = factory.CreateConnection();

            Channel = _connection.CreateModel();
            Channel.ExchangeDeclare(exchange: Exchange,
                type: ExchangeType.Topic);
       }

        public virtual void Dispose()
        {
            Channel.Dispose();
            _connection.Dispose();
        }
    }
}
