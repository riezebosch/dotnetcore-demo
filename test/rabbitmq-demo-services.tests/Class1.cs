using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using rabbitmq_demo;
using rabbitmq_demo_service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo_services.tests
{
    public class Class1
    {
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

        [Fact]
        public void GivenPeopleServiceWhenCreatePersonCommandReceivedThenPersonPersisted()
        {
            using (var context = new DemoContext(CreateOptions()))
            using (var service = new PeopleService(context))
            {
                service.Execute(new CreatePerson { FirstName = "Test", LastName = "Man" });
                var entry = context.ChangeTracker.Entries<Person>().FirstOrDefault();

                Assert.NotNull(entry);
                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }

        ManualResetEvent wait;
        [Fact]
        public void GivenCreatePersonCommandSendWhenServiceIsListeningTheCommandShouldBeProcessed()
        {
            using (ConfigureServices())
            using (var sender = new Sender())
            using (wait = new ManualResetEvent(false))
            {
                sender.Publish(new CreatePerson { FirstName = "test", LastName = "man" });
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
            }
        }

        private IContainer ConfigureServices()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DemoContext>();
            builder.RegisterType<PeopleService>();
            builder.RegisterType<Receiver>();
            builder.RegisterInstance(CreateOptions());

            var container = builder.Build();
            container.Resolve<Receiver>().Subscribe<CreatePerson>(command =>
                {
                    container.Resolve<PeopleService>().Execute(command);
                    wait.Set();
                });

            return container;
        }
    }
}

