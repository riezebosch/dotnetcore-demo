using Autofac;
using NSubstitute;
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

                using (var listener = sender.Listener())
                using (var service = Substitute.For<BlockingReceiver<int>>())
                {
                    service
                        .When(_ => _.Execute(3))
                        .Do(_ => { throw new NotImplementedException(); });
                    service.SubscribeToCommand(listener);

                    Assert.Throws<TimeoutException>(() => service.Next());
                }

                using (var listener = sender.Listener())
                using (var service = new BlockingReceiver<int>())
                {
                    service.SubscribeToCommand(listener);
                    var result = service.Next();

                    Assert.Equal(3, result);
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
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance<IReceive<int>>(service);

                    using (var container = builder.Build())
                    {
                        listener.SubscribeCommands<int>(container);
                        var result = service.Next();

                        Assert.Equal(3, result);
                    }
                }

                using (var listener = sender.Listener())
                using (var service = new BlockingReceiver<int>())
                {
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance(service)
                        .As<IReceive<int>>();

                    using (var container = builder.Build())
                    {
                        listener.SubscribeCommands<int>(container);
                        Assert.Throws<TimeoutException>(() => service.Next(TimeSpan.FromSeconds(1)));
                    }
                }
            }
        }
    }
}
