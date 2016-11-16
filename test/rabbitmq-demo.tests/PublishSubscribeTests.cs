using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Newtonsoft.Json;
using NSubstitute;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class PublishSubscribeTests
    {
        [Fact]
        public void PublishedMessageShouldBeReceivedBySubscribedReceiver()
        {
            // Arrange
            using (var listener = new TestListener())
            using (var receiver = new BlockingReceiver<Person>())
            {
                receiver.SubscribeToEvents(listener);

                // Act
                var input = new Person { FirstName = "Test", LastName = "Man" };
                using (var sender = listener.Sender())
                {
                    sender.PublishEvent(input);
                }

                // Assert
                var result = receiver.Next();
                Assert.Equal(input.FirstName, result.FirstName);
                Assert.Equal(input.LastName, result.LastName);
            }
        }

        [Fact]
        public void ListenerAndSenderShouldConnectWithSpecifiedCredentials()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };
            var connection = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using (var listener = new Listener(connection, "demo2"))
            using (var service = new BlockingReceiver<Person>())
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance<IReceive<Person>>(service);

                using (var container = builder.Build())
                {
                    listener.SubscribeEvents<Person>(container);
                    // Act
                    using (var sender = new Sender(connection, "demo2"))
                    {
                        sender.PublishEvent(input);
                    }

                    // Assert
                    service.Next();
                }
            }
        }


        [Fact]
        public void SubsequentMessageShouldBeReceivedBySubscriber()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new TestListener())
            {
                var service = Substitute.For<IReceive<Person>>();
                service
                    .When(m => m.Execute(Arg.Is<Person>(p => p.FirstName == "first")))
                    .Do(c => wait.Signal());

                service
                    .When(m => m.Execute(Arg.Is<Person>(p => p.FirstName == "second")))
                    .Do(c => wait.Signal());

                var builder = new ContainerBuilder();
                builder.RegisterInstance(service);

                using (var container = builder.Build())
                {
                    listener.SubscribeEvents<Person>(container);
                    using (var sender = listener.Sender())
                    {
                        sender.PublishEvent(new Person { FirstName = "first" });
                        sender.PublishEvent(new Person { FirstName = "second" });
                    }

                    Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                    service.Received(2).Execute(Arg.Any<Person>());
                }
            }
        }

        [Fact]
        public void PublishedMessageShouldBeDeliveredToSubscribedReceiversFromBothListeners()
        {
            // Arrange
            using (var sender = new TestSender())
            using (var listener1 = sender.Listener())
            using (var listener2 = sender.Listener())
            using (var service1 = new BlockingReceiver<Person>())
            using (var service2 = new BlockingReceiver<Person>())
            {
                service1.SubscribeToEvents(listener1);
                service2.SubscribeToEvents(listener2);

                // Act
                sender.PublishEvent(new Person { FirstName = "first" });

                // Assert
                service1.Next();
                service2.Next();
            }
        }

        [Fact]
        public void ListenerReceivesTwoMessagesOfDifferentType()
        {
            using (var listener = new TestListener())
            using (var service1 = new BlockingReceiver<Person>())
            using (var service2 = new BlockingReceiver<string>())
            {
                service1.SubscribeToEvents(listener);
                service2.SubscribeToEvents(listener);

                using (var sender = listener.Sender())
                {
                    sender.PublishEvent(new Person { FirstName = "first" });
                    sender.PublishEvent("just simple text");

                    service1.Next();
                    service2.Next();
                }
            }
        }

        [Fact]
        public void SendAndReceiveShouldNotDependOnClrTypes()
        {
            // Arrange
            var input = new ef_demo.Person { FirstName = "Test", LastName = "Man" };

            using (var listener = new TestListener())
            using (var service = new BlockingReceiver<Person>())
            {
                service.SubscribeToEvents(listener);

                // Act
                using (var sender = listener.Sender())
                {
                    sender.PublishEvent(input);
                }

                // Assert
                var result = service.Next();

                Assert.Equal(input.FirstName, result.FirstName);
                Assert.Equal(input.LastName, result.LastName);
            }
        }

        [Fact]
        public void SendAndReceiveShouldDependOnClassName()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var listener = new TestListener())
            using (var wait = new ManualResetEvent(false))
            using (var service = new BlockingReceiver<SomethingUnrelated>())
            {
                service.SubscribeToEvents(listener);

                // Act
                using (var sender = listener.Sender())
                {
                    sender.PublishEvent(input);
                }

                // Assert
                Assert.Throws<TimeoutException>(() => service.Next(TimeSpan.FromSeconds(1)));
            }
        }

        [Fact]
        public void ListenerRaisesEventsOnReceivingMessages()
        {
            // Arrange
            using (var listener = new TestListener())
            using (var sender = listener.Sender())
            using (var service = new BlockingReceiver<int>())
            {
                var messages = new List<ReceivedEventArgs>();
                listener.Received += (o, e) => messages.Add(e);


                service.SubscribeToEvents(listener);

                // Act
                sender.PublishEvent(3);
                service.Next();

                // Assert
                var message = messages.Single();
                Assert.Equal(service.GetType(), message.HandledBy);
                Assert.Equal("Int32", message.Topic);
                Assert.Equal("3", message.Message);
            }
        }



        [Fact]
        public void ListenerResolvesDependenciesToCreateInstances()
        {
            // Arrange
            using (var listener = new TestListener())
            using (var sender = listener.Sender())
            {
                var dependency = Substitute.For<IDependency>();
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(dependency);
                builder
                    .RegisterReceiverFor<ReceiverWithDependency, int>();

                using (var container = builder.Build())
                {
                    listener.SubscribeEvents<int>(container);
                }
            }
        }

        [Fact]
        public void ListenerThrowsExceptionWhenReceiverForContractIsNotResolved()
        {
            // Arrange
            using (var listener = new Listener(Substitute.For<IConnectionFactory>(), "dummy"))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterType<ReceiverWithDependency>();

                // Act && Assert
                using (var container = builder.Build())
                {
                    Assert.Throws<ComponentNotRegisteredException>(() => listener.SubscribeEvents<int>(container));
                }
            }
        }

        [Fact]
        public void ListenerThrowsExceptionWhenDependencyForReceiverIsNotResolved()
        {
            // Arrange
            using (var listener = new Listener(Substitute.For<IConnectionFactory>(), "dummy"))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterReceiverFor<ReceiverWithDependency, int>();

                // Act && Assert
                using (var container = builder.Build())
                {
                    Assert.Throws<DependencyResolutionException>(() => listener.SubscribeEvents<int>(container));
                }
            }
        }

        [Fact]
        public void ListenerDisposesDependenciesResolvedToCreateInstancesOnSubscribe()
        {
            // Arrange
            using (var listener = new TestListener())
            {
                var dependency = Substitute.For<IDependency, IDisposable>();

                var builder = new ContainerBuilder();
                builder
                    .Register(c => dependency);
                builder
                    .RegisterReceiverFor<ReceiverWithDependency, int>();

                using (var container = builder.Build())
                {
                    // Act
                    listener.SubscribeEvents<int>(container);

                    // Assert
                    ((IDisposable)dependency).Received(1).Dispose();
                }
            }
        }

        [Fact]
        public void ListenerDisposesDependenciesResolvedToCreateInstancesOnExecute()
        {
            // Arrange
            bool disposed = false;
            var dependency = Substitute.For<IDependency, IDisposable>();
            ((IDisposable)dependency).When(_ => _.Dispose()).Do(c => disposed = true);

            var builder = new ContainerBuilder();
            builder
                .Register(c => dependency);
            builder
                .RegisterReceiverFor<ReceiverWithDependency, int>();

            using (var listener = new TestListener())
            using (var sender = listener.Sender())
            using (var container = builder.Build())
            using (var waiter = new BlockingReceiver<int>())
            {
                listener.SubscribeEvents<int>(container);
                dependency.ClearReceivedCalls();

                waiter.SubscribeToEvents(listener);

                // Act
                sender.PublishEvent(4);
                waiter.Next();
            }

            // Assert
            Assert.True(disposed);
        }

        [Fact]
        public void TestListenerProvidesSpecificSender()
        {
            using (var listener1 = new TestListener())
            using (var listener2 = new TestListener())
            using (var service1 = new BlockingReceiver<int>())
            using (var service2 = new BlockingReceiver<int>())
            {
                service1.SubscribeToEvents(listener1);
                service2.SubscribeToEvents(listener2);

                using (var sender = listener1.Sender())
                {
                    sender.PublishEvent(3);
                }

                using (var sender = listener2.Sender())
                {
                    sender.PublishEvent(4);
                }


                Assert.Equal(3, service1.Next());
                Assert.Equal(4, service2.Next());
            }
        }

        [Fact]
        public void TestSenderProvidesSpecificListener()
        {
            using (var sender1 = new TestSender())
            using (var sender2 = new TestSender())
            using (var listener1 = sender1.Listener())
            using (var listener2 = sender2.Listener())
            using (var service1 = new BlockingReceiver<int>())
            using (var service2 = new BlockingReceiver<int>())
            {
                service1.SubscribeToEvents(listener1);
                service2.SubscribeToEvents(listener2);

                sender1.PublishEvent(3);
                sender2.PublishEvent(4);

                Assert.Equal(3, service1.Next());
                Assert.Equal(4, service2.Next());
            }
        }

        [Fact]
        public void SenderRaisesEvent()
        {
            using (var sender = new TestSender())
            {
                var message = string.Empty;
                sender.Send += (o, e) => message = e.Message;

                sender.PublishEvent("hallo");

                Assert.Equal("\"hallo\"", message);
            }
        }
    }
}
