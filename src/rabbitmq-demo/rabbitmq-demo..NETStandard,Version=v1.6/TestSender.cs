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
        public TestSender()
            : base(new ConnectionFactory(), 
                  Guid.NewGuid().ToString())
        {
        }

        public Listener Listener()
        {
            return new Listener(new ConnectionFactory(), 
                Exchange);
        }

        public override void Dispose()
        {
            Channel.ExchangeDelete(Exchange);
            base.Dispose();
        }
    }
}
