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
using System.Threading.Tasks;

namespace rabbitmq_demo
{
    public class TestListener : Listener
    {
        private int _timeout;

        public TestListener() 
            : this(TimeSpan.FromSeconds(10))
        {
        }

        public TestListener(TimeSpan timeout)
            : this(new ConnectionFactory(),
                  Guid.NewGuid().ToString(),
                  (int)timeout.TotalMilliseconds)
        {
        }

        internal TestListener(IConnectionFactory factory, 
            string exchange,
            int timeout) : base(factory, exchange)
        {
            _timeout = timeout;
        }

        public Sender Sender()
        {
            return new TestSender(new ConnectionFactory(), 
                Namespace, 
                _timeout);
        }

        public override void Dispose()
        {
            Channel.ExchangeDelete(Namespace);
            base.Dispose();
        }

        protected override IDictionary<string, object> Arguments => 
            new Dictionary<string, object>
            {
                { "x-expires", _timeout }
            };
    }
}