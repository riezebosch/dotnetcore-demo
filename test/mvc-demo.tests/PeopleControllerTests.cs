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

namespace mvc_demo.tests
{
    public class PeopleControllerTests
    {
        [Fact]
        public void AddPersonPublishesPersonCreatedCommand()
        {
            using (var receiver = new Receiver())
            using (var wait = new ManualResetEvent(false))
            {
                CreatePerson command = null;
                receiver.Subscribe<CreatePerson>(p => { command = p; wait.Set(); });

                var controller = new PeopleController();
                controller.Create("first", "last");

                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.NotNull(command);
                Assert.Equal("first", command.FirstName);
                Assert.Equal("last", command.LastName);
            }
        }

        [Fact]
        public void AddPersonRedirectsToIndex()
        {
            var controller = new PeopleController();
            var result = Assert.IsType<RedirectToActionResult>(controller.Create("first", "last"));
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
        public async Task IntegrationTestOnPostRequest()
        {
            using (var server = StartTestServer())
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsync("People/Create", new StringContent ("first=test;last=test"));
                Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
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
    }
}

