using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabbitmq_demo
{
    public class TestSender : Sender
    {
        private int _timeout;
        
        public TestSender()
            : this(TimeSpan.FromSeconds(10))
        {
        }

        public TestSender(TimeSpan timeout) :
            this(new ConnectionFactory(), 
                  Guid.NewGuid().ToString(),
                  (int)timeout.TotalMilliseconds)
        {
        }

        internal TestSender(IConnectionFactory factory, 
            string exchange, 
            int timeout) : base(factory, exchange)
        {
            _timeout = timeout;
        }

        public Listener Listener()
        {
            return new TestListener(
                new ConnectionFactory(), 
                Exchange,
                _timeout);
        }

        
        public override void Dispose()
        {
            Channel.ExchangeDelete(Exchange);
            base.Dispose();
        }

        protected override IDictionary<string, object> Arguments => 
            new Dictionary<string, object>
            {
                { "x-expires", _timeout }
            };
    }
}
