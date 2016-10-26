using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo_services.tests
{
    public class Class1
    {
        [Fact]
        public void GivenPeopleServiceWhenCreatePersonCommandReceivedThenPersonPersisted()
        {
            DbContextOptions options = new DbContextOptionsBuilder<DemoContext>().UseInMemoryDatabase().Options;
            using (var context = new DemoContext(options))
            using (var service = new PeopleService(context))
            {
                service.Execute(new CreatePerson { FirstName = "Test", LastName = "Man" });
                var entry = context.ChangeTracker.Entries<Person>().FirstOrDefault();

                Assert.NotNull(entry);
                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }
    }
}
