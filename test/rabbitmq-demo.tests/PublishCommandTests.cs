using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class PublishCommandTests
    {
        [Fact]
        public async Task CommandsAreDeliveredAlsoWhenPublishedBeforeListenerSubscribed()
        {
            using (var listener = new TestListener())
            using (var sender = listener.Sender())
            {
                sender.Command(3);

                var awaiter = new ReceiveAsync<int>();
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(awaiter)
                    .As<IReceive<int>>();

                using (var container = builder.Build())
                {
                    listener.Command<int>(container);
                    var result = await awaiter.WithTimeout();

                    Assert.Equal(3, result);
                }
            }
        }

        [Fact]
        public async Task ReceiverWithExceptionLeavesCommandInQueueToBeRedelivered()
        {
            using (var sender = new TestSender())
            {
                sender.Command(3);

                using (var listener = sender.Listener())
                {
                    var awaiter = new ReceiverWithException<int>();
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance<IReceive<int>>(awaiter); ;

                    using (var container = builder.Build())
                    {
                        listener.Command<int>(container);
                        var result = await awaiter.WithTimeout();
                    }
                }

                using (var listener = sender.Listener())
                {
                    var awaiter = new ReceiveAsync<int>();
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance(awaiter)
                        .As<IReceive<int>>();

                    using (var container = builder.Build())
                    {
                        listener.Command<int>(container);
                        var result = await awaiter.WithTimeout();

                        Assert.Equal(3, result);
                    }
                }

            }
        }

        [Fact]
        public async Task ReceivedCommandIsNotDeliveredTwice()
        {
            using (var sender = new TestSender())
            {
                sender.Command(3);

                using (var listener = sender.Listener())
                {
                    var awaiter = new ReceiveAsync<int>();
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance<IReceive<int>>(awaiter); ;

                    using (var container = builder.Build())
                    {
                        listener.Command<int>(container);
                        var result = await awaiter.WithTimeout();
                        Assert.Equal(3, result);
                    }
                }
                using (var listener = sender.Listener())
                {
                    var awaiter = new ReceiveAsync<int>();
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance(awaiter)
                        .As<IReceive<int>>();

                    using (var container = builder.Build())
                    {
                        listener.Command<int>(container);
                        await Assert.ThrowsAsync<TimeoutException>(() => awaiter.WithTimeout(TimeSpan.FromSeconds(1)));
                    }
                }
            }
        }
    }
}
