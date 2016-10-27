using Autofac;
using ef_demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using rabbitmq_demo;
using rabbitmq_demo_service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace rabbitmq_demo_service
{
    public class Services : IDisposable
    {
        private IContainer _container;

        public Services()
        {
            _container = Configure();
        }

        public event Action<object> Received;

        public void Dispose()
        {
            _container.Dispose();
        }

        private IContainer Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DemoContext>();
            builder.RegisterType<PeopleService>();
            builder.RegisterType<Receiver>();
            builder.RegisterInstance(CreateOptions());

            var container = builder.Build();
            container
                .Resolve<Receiver>()
                .Subscribe<CreatePerson>()
                .Then(container.Resolve<PeopleService>().Execute)
                .Then(m => Received?.Invoke(m));

            return container;
        }

        /// <summary>
        /// https://docs.efproject.net/en/latest/miscellaneous/testing.html#writing-tests
        /// </summary>
        /// <returns>Fresh options for every test.</returns>
        private DbContextOptions CreateOptions()
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
    }
}


