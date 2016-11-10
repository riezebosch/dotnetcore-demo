using Autofac;
using Microsoft.EntityFrameworkCore;
using mvc_demo.database;
using RabbitMQ.Client;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mvc_demo.service
{
    public class Program
    {
        public static void Main()
        {
            var builder = new ContainerBuilder();
            builder
                .Register(c => new FrontEndContext(
                    new DbContextOptionsBuilder<FrontEndContext>()
                    .UseSqlServer(@"Server=.\SQLEXPRESS;Database=mvc-demo;Trusted_Connection=true")
                    .Options))
            .As<IFrontEndContext>();

            builder
                .RegisterType<FrontEndService>()
                .As<IReceive<PersonCreated>>();

            using (var listener = new Listener(new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" }, "mvc-demo"))
            using (var container = builder.Build())
            {
                listener.Received += (o, e) => Console.WriteLine(e);

                listener.Subscribe<PersonCreated>(container);
                Console.ReadKey();
            }
        }

    
    }
}
