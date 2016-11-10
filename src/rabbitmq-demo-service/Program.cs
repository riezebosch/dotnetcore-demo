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
        /// <summary>
        /// https://docs.efproject.net/en/latest/miscellaneous/testing.html#writing-tests
        /// </summary>
        /// <returns>Fresh options for every test.</returns>
        private static DbContextOptions CreateOptions()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<DemoContext>();
            builder.UseInMemoryDatabase()
                       .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }
        public static void Main()
        {
            using (var context = new DemoContext(CreateOptions()))
            using (var sender = new Sender())
            {
                var listener = new Listener();
                listener.Received += (o, e) => Console.WriteLine(e);
                listener.Subscribe(new PeopleService(context, sender));

                Console.ReadKey();
            }
        }
    }
}
