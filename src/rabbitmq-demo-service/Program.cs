using Autofac;
using ef_demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
                .RegisterType<PeopleService>()
                .As<IReceive<CreatePerson>>();
            builder
                .RegisterType<DemoContext>();
            builder
                .Register(c =>
                    new DbContextOptionsBuilder<DemoContext>().UseSqlite(@"Filename=.\peopleservice.db").Options);

            using (var listener = new Listener())
            using (var container = builder.Build())
            {
                listener.Received += (o, e) => Console.WriteLine(e);
                listener.Subscribe<CreatePerson>(container);

                Console.ReadKey();
            }
        }
    }
}
