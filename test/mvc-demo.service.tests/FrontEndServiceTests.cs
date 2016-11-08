using Microsoft.EntityFrameworkCore;
using Moq;
using mvc_demo.database;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace mvc_demo.service.tests
{
    public class FrontEndServiceTests
    {
        [Fact]
        public void FrontEndServiceShouldStorePersonCreatedEventsWithMoq()
        {
            // Arrange
            var dbset = new Mock<DbSet<Person>>();
            dbset.Setup(m => m.Add(It.IsAny<Person>())).Verifiable();

            var context = new Mock<IFrontEndContext>();
            context.Setup(m => m.People).Returns(dbset.Object);
            context.Setup(m => m.SaveChanges()).Verifiable();

            var receiver = new Mock<IReceiver>();
            receiver.Setup(m => m.Subscribe(It.IsAny<Action<PersonCreated>>())).Callback<Action<PersonCreated>>(a => a(new PersonCreated { }));

            // Act
            var service = new FrontEndService(context.Object, receiver.Object);

            // Assert
            dbset.Verify();
            context.Verify();
        }

        [Fact]
        public void FrontEndServiceShouldStorePersonCreatedEvents()
        {
            // Arrance
            var dbset = new DummyDbSet<Person>();
            var context = new DummyFrontEndContext(dbset);

            var receiver = new DummyReceiver();

            // Act
            var service = new FrontEndService(context, receiver);

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

        class DummyReceiver : IReceiver
        {
            public void Subscribe<T>(Action<T> action)
            {
                // Simulate the publish by invoking the action
                var input = new PersonCreated { };
                action((T)(object)input);
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

                using (var sender = new Sender())
                using (var receiver = new Receiver())
                {
                    // Act
                    var service = new FrontEndService(context, receiver);
                    receiver.WaitForResult<PersonCreated>(() => sender.Publish(new PersonCreated { }));
                }

                // Assert
                Assert.True(context.People.Any());
            }
        }
    }
}
