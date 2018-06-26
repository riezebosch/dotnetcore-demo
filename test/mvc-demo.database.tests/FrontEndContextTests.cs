using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace mvc_demo.database.tests
{
    public class FrontEndContextTests
    {
        public FrontEndContextTests()
        {
            using (var context = new FrontEndContext(
                new DbContextOptionsBuilder<FrontEndContext>().UseSqlServer(@"Server=.\SQLEXPRESS;Database=mvc-demo.database.tests;Trusted_Connection=true").Options))
            {
                context.Database.Migrate();
            }
        }

        [Fact]
        public void StorePeople()
        {
            using (var context = new FrontEndContext(
                new DbContextOptionsBuilder<FrontEndContext>().UseSqlite(@"File=.\test.db").Options))
            {
                context.People.Add(new Person { Id = 0, Name = "Test Man" });
            }
        }

        [Fact]
        public void IdShouldBeExplicitlySpecifiedWhenAddingNewPerson()
        {
            using (var context = new FrontEndContext(
                           new DbContextOptionsBuilder<FrontEndContext>().UseSqlServer(@"Server=.\SQLEXPRESS;Database=mvc-demo.database.tests;Trusted_Connection=true").Options))
            using (context.Database.BeginTransaction())
            {
                var person = new Person { Id = 15, Name = "Test Man" };
                context.People.Add(person);
                context.SaveChanges();

                Assert.Equal(15, person.Id);
            }
        }

        [Fact]
        public void IdShouldBeUnique()
        {
            using (var connection = new SqlConnection(@"Server=.\SQLEXPRESS;Database=mvc-demo.database.tests;Trusted_Connection=true"))
            {
                connection.Open();

                var options = new DbContextOptionsBuilder<FrontEndContext>()
                    .UseSqlServer(connection)
                    .Options;

                using (var tx = connection.BeginTransaction())
                {
                    using (var context = new FrontEndContext(options))
                    {
                        context.Database.UseTransaction(tx);

                        var person = new Person { Id = 15, Name = "Test Man" };
                        context.People.Add(person);
                        context.SaveChanges();
                    }

                    using (var context = new FrontEndContext(options))
                    {
                        context.Database.UseTransaction(tx);

                        var person = new Person { Id = 15, Name = "Test Man" };
                        context.People.Add(person);

                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                }
            }
        }
    }

    public class FrontEndContextFactory : IDbContextFactory<FrontEndContext>
    {
        public FrontEndContext Create(DbContextFactoryOptions options)
        {
            return new FrontEndContext(new DbContextOptionsBuilder<FrontEndContext>().UseSqlServer(@"Server=.\SQLEXPRESS;Database=mvc-demo.database.tests;Trusted_Connection=true").Options);
        }
    }
}
