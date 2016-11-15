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
        public async Task PublishMessageShouldBeReceivedBySubsriber()
        {
            // Arrange
            using (var listener = new TestListener())
            {
                var service = new ReceiveAsync<Person>();
                service.SubscribeToEvents(listener);

                // Act
                var input = new Person { FirstName = "Test", LastName = "Man" };
                using (var sender = listener.Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                var result = await service.WithTimeout();
                Assert.Equal(input.FirstName, result.FirstName);
                Assert.Equal(input.LastName, result.LastName);
            }
        }

        [Fact]
        public async Task ConnectWithCredentials()
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
            {
                var service = new ReceiveAsync<Person>();
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance<IReceive<Person>>(service);

                using (var container = builder.Build())
                {
                    listener.SubscribeEvents<Person>(container);
                    // Act
                    using (var sender = new Sender(connection, "demo2"))
                    {
                        sender.Publish(input);
                    }

                    // Assert
                    await service.WithTimeout();
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
                        sender.Publish(new Person { FirstName = "first" });
                        sender.Publish(new Person { FirstName = "second" });
                    }

                    Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                    service.Received(2).Execute(Arg.Any<Person>());
                }
            }
        }

        [Fact]
        public async Task TwoListenersBothReceiveMessageAfterPublish()
        {
            // Arrange
            using (var sender = new TestSender())
            using (var listener1 = sender.Listener())
            using (var listener2 = sender.Listener())
            {
                var service1 = new ReceiveAsync<Person>();
                service1.SubscribeToEvents(listener1);

                var service2 = new ReceiveAsync<Person>();
                service2.SubscribeToEvents(listener2);

                // Act
                sender.Publish(new Person { FirstName = "first" });

                // Assert
                await Task.WhenAll(service1.WithTimeout(), service2.WithTimeout());
            }
        }

        [Fact]
        public async Task OneListenerReceivesTwoMessagesOfDifferentType()
        {
            using (var listener = new TestListener())
            {
                var service1 = new ReceiveAsync<Person>();
                service1.SubscribeToEvents(listener);

                var service2 = new ReceiveAsync<string>();
                service2.SubscribeToEvents(listener);

                using (var sender = listener.Sender())
                {
                    sender.Publish(new Person { FirstName = "first" });
                    sender.Publish("just simple text");

                    await Task.WhenAll(service1.WithTimeout(), service2.WithTimeout());
                }
            }
        }

        [Fact]
        public async Task SendAndReceiveShouldNotDependOnClrTypes()
        {
            // Arrange
            var input = new ef_demo.Person { FirstName = "Test", LastName = "Man" };

            using (var listener = new TestListener())
            {
                var service = new ReceiveAsync<Person>();
                service.SubscribeToEvents(listener);

                // Act
                using (var sender = listener.Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                var result = await service.WithTimeout();

                Assert.Equal(input.FirstName, result.FirstName);
                Assert.Equal(input.LastName, result.LastName);
            }
        }

        [Fact]
        public async Task SendAndReceiveShouldDependOnClassName()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var listener = new TestListener())
            using (var wait = new ManualResetEvent(false))
            {
                var service = new ReceiveAsync<SomethingUnrelated>();
                service.SubscribeToEvents(listener);

                // Act
                using (var sender = listener.Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                await Assert.ThrowsAsync<TimeoutException>(() => service.WithTimeout());
            }
        }

        [Fact]
        public async Task ListenerRaisesEventsOnReceivingMessages()
        {
            // Arrange
            using (var listener = new TestListener())
            using (var sender = listener.Sender())
            {
                var messages = new List<ReceivedEventArgs>();
                listener.Received += (o, e) => messages.Add(e);

                var service = new ReceiveAsync<int>();
                service.SubscribeToEvents(listener);

                // Act
                sender.Publish(3);
                await service.WithTimeout();

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
        public async Task ListenerDisposesDependenciesResolvedToCreateInstancesOnExecute()
        {
            // Arrange
            using (var listener = new TestListener())
            using (var sender = listener.Sender())
            {
                var dependency = Substitute.For<IDependency, IDisposable>();

                var builder = new ContainerBuilder();
                builder
                    .Register(c => dependency);
                builder
                    .RegisterReceiverFor<ReceiverWithDependency, int>();

                using (var container = builder.Build())
                {
                    listener.SubscribeEvents<int>(container);
                    dependency.ClearReceivedCalls();

                    var waiter = new ReceiveAsync<int>();
                    waiter.SubscribeToEvents(listener);

                    // Act
                    sender.Publish(4);

                    // Assert
                    await waiter.WithTimeout();
                    ((IDisposable)dependency).Received(1).Dispose();
                }
            }
        }

        [Fact]
        public async Task TestListenerProvidesSpecificSender()
        {
            using (var listener1 = new TestListener())
            using (var listener2 = new TestListener())
            {
                var service1 = new ReceiveAsync<int>();
                service1.SubscribeToEvents(listener1);

                var service2 = new ReceiveAsync<int>();
                service2.SubscribeToEvents(listener2);

                using (var sender = listener1.Sender())
                {
                    sender.Publish(3);
                }

                using (var sender = listener2.Sender())
                {
                    sender.Publish(4);
                }


                Assert.Equal(3, await service1.WithTimeout());
                Assert.Equal(4, await service2.WithTimeout());
            }
        }

        [Fact]
        public async Task TestSenderProvidesSpecificListener()
        {
            using (var sender1 = new TestSender())
            using (var sender2 = new TestSender())
            using (var listener1 = sender1.Listener())
            using (var listener2 = sender2.Listener())
            {
                var service1 = new ReceiveAsync<int>();
                service1.SubscribeToEvents(listener1);

                var service2 = new ReceiveAsync<int>();
                service2.SubscribeToEvents(listener2);

                sender1.Publish(3);
                sender2.Publish(4);

                Assert.Equal(3, await service1.WithTimeout());
                Assert.Equal(4, await service2.WithTimeout());
            }
        }

        [Fact]
        public void SenderRaisesEvent()
        {
            using (var sender = new TestSender())
            using (var listener = sender.Listener())
            {
                var message = string.Empty;
                sender.Send += (o, e) => message = e.Message;

                sender.Publish("hallo");

                Assert.Equal("\"hallo\"", message);
            }
        }
    }
}
