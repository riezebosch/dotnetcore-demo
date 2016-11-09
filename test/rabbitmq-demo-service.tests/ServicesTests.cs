using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace rabbitmq_demo_service.tests
{
    public class ServicesTests
    {
        [Fact]
        public void GivenCreatePersonCommandSendWhenServiceIsListeningTheCommandShouldBeProcessed()
        {
            using (var services = new Services())
            using (var sender = new Sender())
            using (var wait = new ManualResetEvent(false))
            {
                services.Received += m => wait.Set();

                sender.Publish(new CreatePerson { FirstName = "test", LastName = "man" });
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
            }
        }
    }
}


