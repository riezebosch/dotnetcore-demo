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
using NSubstitute;
using RabbitMQ.Client;

namespace mvc_demo.tests
{
    public class PeopleControllerTests
    {
        [Fact]
        public async Task AddPersonPublishesPersonCreatedCommand()
        {
            using (var listener = new Listener())
            using (var sender = listener.Sender())
            {
                var controller = new PeopleController(sender);
                var receiver = new ReceiveAsync<CreatePerson>();
                listener.Subscribe(receiver);

                controller.Create(new CreatePerson { FirstName = "first", LastName = "last" });
                var command = await receiver.WithTimeout();

                Assert.NotNull(command);
                Assert.Equal("first", command.FirstName);
                Assert.Equal("last", command.LastName);
            }
        }

        [Fact]
        public void AddPersonRedirectsToIndex()
        {
            var controller = new PeopleController(Substitute.For<ISender>());
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
        public async Task IntegrationTestOnPostRequest()
        {
            using (var server = StartTestServer())
            using (var client = server.CreateClient())
            using (var listener = new Listener(new ConnectionFactory(), "mvc-demo"))
            {
                var receiver = new ReceiveAsync<CreatePerson>();
                listener.Subscribe(receiver);

                var result = client.PostAsync("People/Create", new StringContent(JsonConvert.SerializeObject(new { FirstName = "first", LastName = "last" }), Encoding.UTF8, "application/json")).Result;
                var command = await receiver.WithTimeout();

                Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
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
    }
}

