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
        protected string Exchange { get; }

        private IConnection _connection;
        private readonly bool delete;
        private bool _delete;

        protected IModel Channel { get; private set; }

        public ChannelBase() 
            : this(new ConnectionFactory(), Guid.NewGuid().ToString(), true)
        {
        }

        public ChannelBase(IConnectionFactory factory, string exchange)
            : this(factory, exchange, false)
        {
        }
        private ChannelBase(IConnectionFactory factory, string exchange, bool delete)
        {
            Exchange = exchange;
            _connection = factory.CreateConnection();
            _delete = delete;

            Channel = _connection.CreateModel();
            Channel.ExchangeDeclare(exchange: Exchange,
                type: ExchangeType.Topic);
        }

        public void Dispose()
        {
            if (_delete)
            {
                Channel.ExchangeDelete(Exchange);
            }

            Channel.Dispose();
            _connection.Dispose();
        }
    }
}
