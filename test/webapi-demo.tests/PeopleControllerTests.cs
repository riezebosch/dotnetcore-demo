using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace webapi_demo.tests
{
    public class PeopleControllerTests
    {
        [Fact]
        public async Task TestGetReturnsListOfPeople()
        {
            using (var context = await InitializeDatabase())
            {
                var controller = new PeopleController(context);
                var result = controller.Get();

                Assert.Contains<Person>(result, p => p.FirstName == "Manuel" && p.LastName == "Riezebosch");
            }
        }

        private static async Task<DemoContext> InitializeDatabase(string file = "./test.db")
        {
            var builder = new DbContextOptionsBuilder<DemoContext>();
            builder.UseSqlite($"FileName={file}");

            var context = new DemoContext(builder.Options);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            context.People.Add(new Person { Id = 0, FirstName = "Manuel", LastName = "Riezebosch" });
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task IntegrationTestForPeople()
        {
            using (var context = await InitializeDatabase(file: "./temp.db"))
            {
            }

            var server = new TestServer(new WebHostBuilder().UseStartup<Startup>().ConfigureServices(services => services.AddDbContext<DemoContext>(options => options.UseSqlite("FileName='./temp.db'"))));
            var client = server.CreateClient();

            var response = await client.GetAsync("/api/people");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.Contains("Manuel", result);

        }
    }
}
