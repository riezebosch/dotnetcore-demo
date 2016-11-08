using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace mvc_demo.database.tests
{
    public class FrontEndContextTests
    {
        [Fact]
        public void StorePeople()
        {
            using (var context = new FrontEndContext(
                new DbContextOptionsBuilder<FrontEndContext>().UseSqlite(@"File=.\test.db").Options))
            {
                context.People.Add(new Person { Id = 0, Name = "Test Man" });

            }
        }
    }
}
