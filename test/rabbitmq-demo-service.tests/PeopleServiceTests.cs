using ef_demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo_service.tests
{
    public class PeopleServiceTests
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
            {
                var service = new PeopleService(context, new Mock<ISender>().Object);
                service.Execute(new CreatePerson { FirstName = "Test", LastName = "Man" });
                var entry = context.ChangeTracker.Entries<Person>().FirstOrDefault();

                Assert.NotNull(entry);
                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }

        [Fact]
        public void WhenExecutingCreatePersonCommandPersonCreatedEventIsRaised()
        {
            var sender = new Mock<ISender>();
            sender.Setup(
                m => m.PublishEvent(It.Is<PersonCreated>(a =>
                    a.Id != 0
                    && a.FirstName == "Test"
                    && a.LastName == "Man")))
                .Verifiable();

            using (var context = new DemoContext(CreateOptions()))
            {
                var service = new PeopleService(context, sender.Object);
                service.Execute(new CreatePerson { FirstName = "Test", LastName = "Man" });
            }

            sender.Verify();
        }
    }
}


