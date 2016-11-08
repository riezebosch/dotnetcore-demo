using Controllers;
using Microsoft.AspNetCore.Mvc;
using Model.Commands;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace mvc_demo.tests
{
    public class PeopleControllerTests
    {
        [Fact]
        public void AddPersonPublishesPersonCreatedCommand()
        {
            var controller = new PeopleController();
            var command = WaitForResult<CreatePerson>(() => controller.Create(new CreatePerson { FirstName = "first", LastName = "last" }));

            Assert.NotNull(command);
            Assert.Equal("first", command.FirstName);
            Assert.Equal("last", command.LastName);
        }

        [Fact]
        public void AddPersonRedirectsToIndex()
        {
            var controller = new PeopleController();
            var result = Assert.IsType<RedirectToActionResult>(controller.Create(new CreatePerson { FirstName = "first", LastName = "last" }));

            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task OnlyAcceptPostRequestOnCreateAction()
        {
            using (var server = StartTestServer())
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync("People/Create?first=test&last=test");
                Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            }
        }

        [Fact]
        public void IntegrationTestOnPostRequest()
        {
            using (var server = StartTestServer())
            using (var client = server.CreateClient())
            {
                var command = WaitForResult<CreatePerson>(() =>
                {
                    var result = client.PostAsync("People/Create", new StringContent(JsonConvert.SerializeObject(new { FirstName = "first", LastName = "last" }), Encoding.UTF8, "application/json")).Result;
                    Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
                });

                Assert.Equal("first", command.FirstName);
                Assert.Equal("last", command.LastName);
            }
        }


        private static TestServer StartTestServer()
        {
            return new TestServer(new WebHostBuilder()
                            .UseStartup<Startup>()
                            .UseContentRoot(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "src", "mvc-demo"))
                            .ConfigureServices(services =>
                            {
                            }));
        }

        private static T WaitForResult<T>(Action publish)
        {
            T result = default(T);
            using (var receiver = new Receiver())
            using (var wait = new ManualResetEvent(false))
            {
                receiver.Subscribe<T>(p => { result = p; wait.Set(); });
                publish();

                if (!wait.WaitOne(TimeSpan.FromSeconds(5)))
                {
                    throw new TimeoutException();
                }
            }

            return result;
        }
    }
}

