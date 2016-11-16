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
using Microsoft.Extensions.DependencyInjection;

namespace mvc_demo.tests
{
    public class PeopleControllerTests
    {
        [Fact]
        public void AddPersonPublishesPersonCreatedCommand()
        {
            var sender = Substitute.For<ISender>();
            var controller = new PeopleController(sender);

            var command = new CreatePerson { FirstName = "first", LastName = "last" };
            controller.Create(command);

            sender.Received(1).PublishCommand(command);
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
        public void IntegrationTestOnPostRequest()
        {
            using (var service = new BlockingReceiver<CreatePerson>())
            using (var listener = new Listener(new ConnectionFactory(), "mvc-demo"))
            using (var server = StartTestServer())
            using (var client = server.CreateClient())
            {
                service.SubscribeToCommand(listener);

                var result = client.PostAsync("People/Create", new StringContent(JsonConvert.SerializeObject(new { FirstName = "first", LastName = "last" }), Encoding.UTF8, "application/json")).Result;
                Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);

                var command = service.Next();
                Assert.Equal("first", command.FirstName);
                Assert.Equal("last", command.LastName);
            }
        }


        private static TestServer StartTestServer()
        {
            return new TestServer(new WebHostBuilder()
                            .UseStartup<Startup>()
                            .UseContentRoot(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "src", "mvc-demo")));
        }
    }
}

