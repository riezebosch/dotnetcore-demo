using Microsoft.EntityFrameworkCore;
using Moq;
using mvc_demo.database;
using rabbitmq_demo;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
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

            var service = new FrontEndService(context.Object);

            // Act
            service.Execute(new PersonCreated { });
            
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
        public async Task FrontEndServiceShouldRespondToPersonCreatedEvents()
        {
            // Arrange
            using (var context = new FrontEndContext(
                new DbContextOptionsBuilder<FrontEndContext>().UseSqlite(@"Filename=.\test.db").Options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                using (var sender = new TestSender())
                using (var receiver = sender.Listener())
                {
                    // Act
                    var service = new FrontEndService(context);
                    receiver.Subscribe(service);

                    var task = new ReceiveAsync<PersonCreated>();
                    receiver.Subscribe(task);

                    sender.Publish(new PersonCreated { });
                    await task;
                }

                // Assert
                Assert.True(context.People.Any());
            }
        }
    }
}
