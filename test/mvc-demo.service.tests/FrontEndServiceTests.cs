using Microsoft.EntityFrameworkCore;
using mvc_demo.database;
using rabbitmq_demo;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Autofac;
using System.Threading;
using NSubstitute;

namespace mvc_demo.service.tests
{
    public class FrontEndServiceTests
    {
        [Fact]
        public void FrontEndServiceShouldStorePersonCreatedEventsWithMoq()
        {
            // Arrange
            var dbset = Substitute.For<DbSet<Person>>();
            var context = Substitute.For<IFrontEndContext>();
            context.People.Returns(dbset);

            var service = new FrontEndService(context);

            // Act
            service.Execute(new PersonCreated { });

            // Assert
            dbset.Received(1).Add(Arg.Any<Person>());
            context.Received(1).SaveChanges();
        }

        [Fact]
        public void FrontEndServiceShouldStorePersonCreatedEvents()
        {
            // Arrance
            var dbset = new DummyDbSet<Person>();
            var context = new DummyFrontEndContext(dbset);
            var service = new FrontEndService(context);

            // Act
            service.Execute(new PersonCreated { });

            // Assert
            Assert.True(dbset.AddIsCalled);
            Assert.True(context.SaveChangedIsCalled);
        }

        class DummyFrontEndContext : IFrontEndContext
        {
            public DummyFrontEndContext(DummyDbSet<Person> dbset)
            {
                People = dbset;
            }

            public DbSet<Person> People { get; private set; }
            public bool SaveChangedIsCalled { get; internal set; }

            public int SaveChanges()
            {
                SaveChangedIsCalled = true;
                return 0;
            }

        }

        class DummyDbSet<T> : DbSet<T>
            where T : class
        {
            public bool AddIsCalled { get; private set; }

            public override EntityEntry<T> Add(T entity)
            {
                AddIsCalled = true;
                return null;
            }
        }


        [Fact]
        [Trait("type", "integration")]
        public void FrontEndServiceShouldRespondToPersonCreatedEvents()
        {
            // Arrange
            using (var context = new FrontEndContext(
                new DbContextOptionsBuilder<FrontEndContext>().UseSqlite(@"Filename=.\test.db").Options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var builder = new ContainerBuilder();
                builder
                    .RegisterType<FrontEndService>()
                    .As<IReceive<PersonCreated>>();
                builder
                    .RegisterInstance<IFrontEndContext>(context).ExternallyOwned();

                using (var waiter = new BlockingReceiver<PersonCreated>())
                using (var container = builder.Build())
                using (var sender = new TestSender())
                using (var listener = sender.Listener())
                {
                    listener.SubscribeEvents<PersonCreated>(container);
                    waiter.SubscribeToEvents(listener);

                    // Act
                    sender.PublishEvent(new PersonCreated { });
                    waiter.Next();
                }

                // Assert
                Assert.True(context.People.Any());
            }
        }
    }
}
