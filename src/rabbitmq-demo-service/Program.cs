using Autofac;
using ef_demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rabbitmq_demo_service
{
    public class Program
    {
        public static void Main()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterReceiverFor<PeopleService, CreatePerson>();
            builder
                .RegisterType<DemoContext>();
            builder
                .Register(c =>
                    new DbContextOptionsBuilder<DemoContext>().UseSqlite(@"Filename=.\peopleservice.db").Options);

            using (var listener = new Listener(new ConnectionFactory(), "mvc-demo"))
            using (var container = builder.Build())
            {
                listener.Received += (o, e) => Console.WriteLine(e);
                listener.SubscribeEvents<CreatePerson>(container);

                Console.ReadKey();
            }
        }
    }
}
