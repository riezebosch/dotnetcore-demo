using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ef_demo.tests
{
    /// <summary>
    /// With the help of: https://xunit.github.io/docs/getting-started-dotnet-core.html
    /// </summary>
    public class Class1
    {
        [Fact]
        public async Task TestEFCoreCreatesSqliteDatabase()
        {
            var builder = new DbContextOptionsBuilder<DemoContext>();
            builder.UseSqlite("FileName=./test.db");

            using (var context = new DemoContext(builder.Options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.People.Add(new Person { Id = 0, FirstName = "Manuel", LastName = "Riezebosch" });
                await context.SaveChangesAsync();
            }


            using (var context = new DemoContext(builder.Options))
            {
                Assert.Single(context.People);
            }
        }
    }
}
