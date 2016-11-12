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
    internal class MessageReceivedHandler<TContract>
    {
        private readonly IContainer container;
        private readonly Action<Type, string> received;

        public MessageReceivedHandler(IContainer container, Action<Type, string> received)
        {
            this.container = container;
            this.received = received;
        }

        public void Handle(object sender, BasicDeliverEventArgs ea)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var receiver = scope.Resolve<IReceive<TContract>>();
                var content = ea.Body.ToContent();

                received(receiver.GetType(), content);
                receiver.Execute(content.ToObject<TContract>());
            }
        }
    }
}
