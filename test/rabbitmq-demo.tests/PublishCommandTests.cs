using Autofac;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using NSubstitute.ExceptionExtensions;
using System.Threading;

namespace rabbitmq_demo.tests
{
    public class PublishCommandTests
    {
        [Fact]
        public void CommandsAreDeliveredAlsoWhenPublishedBeforeListenerSubscribed()
        {
            using (var listener = new TestListener())
            using (var sender = listener.Sender())
            using (var service = new BlockingReceiver<int>())
            {
                sender.Command(3);

                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(service)
                    .As<IReceive<int>>();

                using (var container = builder.Build())
                {
                    listener.SubscribeCommands<int>(container);
                    var result = service.Next();

                    Assert.Equal(3, result);
                }
            }
        }

        [Fact]
        public void ReceiverWithExceptionLeavesCommandInQueueToBeRedelivered()
        {
            using (var sender = new TestSender())
            {
                sender.Command(3);

                using (var wait = new ManualResetEvent(false))
                {
                    var service = Substitute.For<IReceive<int>>();
                    service.When(_ => _.Execute(Arg.Any<int>())).Do(_ =>
                    {
                        wait.Set();
                        throw new NotImplementedException();
                    });

                    using (var listener = sender.Listener())
                    {
                        var builder = new ContainerBuilder();
                        builder.RegisterInstance(service);

                        using (var container = builder.Build())
                        {
                            listener.SubscribeCommands<int>(container);
                            Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                        }

                        service.Received(1).Execute(3);
                    }
                }

                using (var listener = sender.Listener())
                using (var service = new BlockingReceiver<int>())
                {
                    service.SubscribeToCommand(listener);
                    Assert.Equal(3, service.Next());
                }

            }
        }

        [Fact]
        public void ListenerRaisesEventsOnReceivingCommands()
        {
            // Arrange
            using (var listener = new TestListener())
            using (var sender = listener.Sender())
            using (var service = new BlockingReceiver<int>())
            {
                var messages = new List<ReceivedEventArgs>();
                listener.Received += (o, e) => messages.Add(e);

                service.SubscribeToCommand(listener);

                // Act
                sender.Command(3);
                service.Next();

                // Assert
                var message = messages.Single();
                Assert.Equal(service.GetType(), message.HandledBy);
                Assert.Equal("Int32", message.Topic);
                Assert.Equal("3", message.Message);
            }
        }

        [Fact]
        public void ReceivedCommandIsNotDeliveredTwice()
        {
            using (var sender = new TestSender())
            {
                sender.Command(3);

                using (var listener = sender.Listener())
                using (var service = new BlockingReceiver<int>())
                {
                    service.SubscribeToCommand(listener);
                    Assert.Equal(3, service.Next());
                }

                using (var listener = sender.Listener())
                using (var service = new BlockingReceiver<int>())
                {
                    service.SubscribeToCommand(listener);
                    Assert.Throws<TimeoutException>(() => service.Next(TimeSpan.FromSeconds(1)));
                }
            }
        }

        [Fact]
        public void ChannelBaseUsesPrivateQueuesPerNamespace()
        {
            using (var sender1 = new TestSender())
            using (var sender2 = new TestSender())
            {
                sender1.Command(3);
                sender2.Command(4);

                using (var listener = sender1.Listener())
                using (var service = new BlockingReceiver<int>())
                {
                    service.SubscribeToCommand(listener);
                    Assert.Equal(3, service.Next());
                    Assert.Throws<TimeoutException>(() => service.Next(TimeSpan.FromSeconds(1)));
                }

                using (var listener = sender2.Listener())
                using (var service = new BlockingReceiver<int>())
                {
                    service.SubscribeToCommand(listener);
                    Assert.Equal(4, service.Next());
                    Assert.Throws<TimeoutException>(() => service.Next(TimeSpan.FromSeconds(1)));
                }
            }
        }

        [Fact]
        public void TestListenerAndSenderRemovesCommandQueues()
        {
            var timeout = TimeSpan.FromSeconds(1);
            using (var sender = new TestSender(timeout))
            {
                sender.Command(6);
                using (var listener = sender.Listener())
                {
                    using (var service = new BlockingReceiver<int>())
                    {
                        service.SubscribeToCommand(listener);
                        Assert.Equal(6, service.Next());
                    }
                }

                sender.Command(3);
                using (var listener = sender.Listener())
                {
                    Thread.Sleep(timeout);
                    using (var service = new BlockingReceiver<int>())
                    {
                        service.SubscribeToCommand(listener);
                        Assert.Throws<TimeoutException>(() => service.Next(TimeSpan.FromSeconds(1)));
                    }
                }
            }
        }
    }
}
