using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class ReceiverAsyncTests
    {
        [Fact]
        public async Task WaitForResult()
        {
            using (var listener = new Listener())
            using (var sender = listener.Sender())
            {
                var receiver = new ReceiveAsync<int>();
                listener.Subscribe(receiver);

                sender.Publish(3);

                var result = await receiver;
                Assert.Equal(3, result);
            }
        }

        [Fact]
        public async Task WaitForResultWithTimeoutThrowsException()
        {
            using (var listener = new Listener())
            using (var sender = listener.Sender())
            {
                var receiver = new ReceiveAsync<int>();
                listener.Subscribe(receiver);

                await Assert.ThrowsAsync<TimeoutException>(() => receiver.WithTimeout(TimeSpan.FromSeconds(1)));
            }
        }

       
    }
}
