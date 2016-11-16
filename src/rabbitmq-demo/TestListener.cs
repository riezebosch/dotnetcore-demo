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
        public TestListener() 
            : base(new ConnectionFactory(), 
                  Guid.NewGuid().ToString())
        {
        }

        public Sender Sender()
        {
            return new Sender(new ConnectionFactory(), Exchange);
        }

        public override void Dispose()
        {
            Channel.ExchangeDelete(Exchange);
            base.Dispose();
        }
    }
}