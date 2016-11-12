using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using NSubstitute;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var listener = new Listener())
            {
                var service = Substitute.For<IReceive<Person>>();

                // Act
                listener
                    .Subscribe(service);

                var waiter = new ReceiveAsync<Person>();
                listener.Subscribe(waiter);

                using (var sender = listener.Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                await waiter.WithTimeout();
                service
                    .Received()
                    .Execute(Arg.Is<Person>(p =>
                        p.FirstName == input.FirstName
                        && p.LastName == input.LastName));
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
                var waiter = new ReceiveAsync<Person>();
                listener.Subscribe(waiter);

                // Act
                using (var sender = new Sender(connection, "demo2"))
                {
                    sender.Publish(input);
                }

                // Assert
                await waiter.WithTimeout();
            }
        }


        [Fact]
        public void SubsequentMessageShouldBeReceivedBySubscriber()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Listener())
            {
                var service = Substitute.For<IReceive<Person>>();
                service
                    .When(m => m.Execute(Arg.Is<Person>(p => p.FirstName == "first")))
                    .Do(c => wait.Signal());

                service
                    .When(m => m.Execute(Arg.Is<Person>(p => p.FirstName == "second")))
                    .Do(c => wait.Signal());

                listener.Subscribe(service);

                using (var sender = listener.Sender())
                {
                    sender.Publish(new Person { FirstName = "first" });
                    sender.Publish(new Person { FirstName = "second" });
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                service.Received(2).Execute(Arg.Any<Person>());
            }
        }

        [Fact]
        public void TwoListenersBothReceiveMessageAfterPublish()
        {
            using (var wait = new CountdownEvent(2))
            using (var sender = new Sender())
            using (var listener1 = sender.Listener())
            using (var listener2 = sender.Listener())
            {
                var service = Substitute.For<IReceive<Person>>();
                service
                    .When(m => m.Execute(Arg.Any<Person>()))
                    .Do(c => wait.Signal());

                listener1.Subscribe(service);
                listener2.Subscribe(service);


                sender.Publish(new Person { FirstName = "first" });


                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                service.Received(2).Execute(Arg.Any<Person>());
            }
        }

        [Fact]
        public void OneListenerReceivesTwoMessagesOfDifferentType()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Listener())
            {
                var service1 = Substitute.For<IReceive<Person>>();
                service1
                    .When(m => m.Execute(Arg.Any<Person>()))
                    .Do(c => wait.Signal());

                var service2 = Substitute.For<IReceive<Person>>();
                service2
                    .When(m => m.Execute(Arg.Any<Person>()))
                    .Do(c => wait.Signal());

                listener.Subscribe(service1);
                listener.Subscribe(service2);

                using (var sender = listener.Sender())
                {
                    sender.Publish(new Person { FirstName = "first" });
                    sender.Publish("just simple text");
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                service1.Received(1).Execute(Arg.Any<Person>());
                service2.Received(1).Execute(Arg.Any<Person>());
            }
        }

        [Fact]
        public async Task SendAndReceiveShouldNotDependOnClrTypes()
        {
            // Arrange
            var input = new ef_demo.Person { FirstName = "Test", LastName = "Man" };

            using (var listener = new Listener())
            {
                var service = Substitute.For<IReceive<Person>>();
                listener.Subscribe(service);

                var waiter = new ReceiveAsync<Person>();
                listener.Subscribe(waiter);

                // Act
                using (var sender = listener.Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                await waiter.WithTimeout();
                service
                    .Received(1)
                    .Execute(Arg.Is<Person>(other =>
                        other.FirstName == input.FirstName
                        && other.LastName == input.LastName));
            }
        }

        [Fact]
        public async Task SendAndReceiveShouldDependOnClassName()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var listener = new Listener())
            using (var wait = new ManualResetEvent(false))
            {
                var mock = Substitute.For<IReceive<SomethingUnrelated>>();
                listener.Subscribe(mock);

                var waiter = new ReceiveAsync<Person>();
                listener.Subscribe(waiter);

                // Act
                using (var sender = listener.Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                await waiter.WithTimeout();
                mock.DidNotReceive().Execute(Arg.Any<SomethingUnrelated>());
            }
        }

        [Fact]
        public async Task ListenerRaisesEvent()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = listener.Sender())
            {
                var messages = new List<ReceivedEventArgs>();
                listener.Received += (o, e) => messages.Add(e);

                var receiver = new ReceiveAsync<int>();
                listener.Subscribe(receiver);

                // Act
                sender.Publish(3);
                await receiver.WithTimeout();

                // Assert
                var message = messages.Single();
                Assert.Equal(receiver.GetType(), message.HandledBy);
                Assert.Equal("Int32", message.Topic);
                Assert.Equal("3", message.Message);
            }
        }

        [Fact]
        public void ListenerUsesFactoryToCreateInstances()
        {
            // Arrange
            using (var listener = new Listener())
            {
                Func<IReceive<int>> factory = Substitute.For<Func<IReceive<int>>>();

                // Act
                listener
                    .Subscribe(factory);

                // Assert
                factory.Received().Invoke();
            }
        }

        [Fact]
        public void ListenerDoesNotDisposeInstanceSubscribedReceivers()
        {
            // Arrange
            using (var listener = new Listener())
            {
                var service = Substitute.For<IReceive<int>, IDisposable>();

                // Act
                listener
                    .Subscribe(service);

                // Assert
                ((IDisposable)service).DidNotReceive().Dispose();
            }
        }

        [Fact]
        public void ListenerDisposesFactoryCreatedReceivers()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var service = Substitute.For<IReceive<int>, IDisposable>();

                // Act
                listener.Subscribe(() => service);

                // Assert
                ((IDisposable)service).Received(1).Dispose();
            }
        }



        [Fact]
        public void ListenerThrowsExceptionWhenReceiverForContractIsNotResolved()
        {
            // Arrange
            using (var listener = new Listener())
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterType<ReceiverWithDependency>();

                // Act && Assert
                Assert.Throws<ComponentNotRegisteredException>(
                    () => listener.Subscribe<int>(builder.Build()));
            }
        }

        [Fact]
        public void ListenerThrowsExceptionWhenDependencyForReceiverIsNotResolved()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterReceiverFor<ReceiverWithDependency, int>();

                // Act && Assert
                Assert.Throws<DependencyResolutionException>(() => listener.Subscribe<int>(builder.Build()));
            }
        }

        [Fact]
        public void ListenerDisposesDependenciesResolvedToCreateInstances()
        {
            // Arrange
            using (var listener = new Listener())
            {
                var dependency = Substitute.For<IDependency, IDisposable>();

                var builder = new ContainerBuilder();
                builder.Register(c => dependency);

                builder
                    .RegisterReceiverFor<ReceiverWithDependency, int>();

                using (var container = builder.Build())
                {
                    // Act
                    listener.Subscribe<int>(container);

                    // Assert
                    ((IDisposable)dependency).Received(1).Dispose();
                }
            }
        }

        [Fact]
        public async Task ListenerResolvesDependenciesToCreateInstances()
        {
            // Arrange
            using (var listener = new Listener())
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
                    listener.Subscribe<int>(container);

                    var waiter = new ReceiveAsync<int>();
                    listener.Subscribe(waiter);

                    // Act
                    sender.Publish(3);
                    await waiter.WithTimeout();

                    // Assert
                    dependency.Received(1).Foo();
                }
            }
        }

        [Fact]
        public void TestSenderIsSpecificForReceiver()
        {
            using (var listener1 = new Listener())
            using (var listener2 = new Listener())
            {
                var service1 = Substitute.For<IReceive<int>>();
                var service2 = Substitute.For<IReceive<int>>();

                listener1.Subscribe(service1);
                listener2.Subscribe(service2);

                using (var sender = listener1.Sender())
                {
                    sender.Publish(3);
                }

                using (var sender = listener2.Sender())
                {
                    sender.Publish(4);
                }

                service1.Received(1).Execute(3);
                service2.Received(1).Execute(4);
            }
        }

        [Fact]
        public async Task TestReceiverIsSpecificForSender()
        {
            using (var sender1 = new Sender())
            using (var sender2 = new Sender())
            using (var listener1 = sender1.Listener())
            using (var listener2 = sender2.Listener())
            {
                var service1 = new ReceiveAsync<int>();
                var service2 = new ReceiveAsync<int>();

                listener1.Subscribe(service1);
                listener2.Subscribe(service2);

                sender1.Publish(3);
                sender2.Publish(4);

                Assert.Equal(3, await service1.WithTimeout());
                Assert.Equal(4, await service2.WithTimeout());
            }
        }

        [Fact]
        public void SenderRaisesEvent()
        {
            using (var sender = new Sender())
            using (var listener = new Listener())
            {
                var message = string.Empty;
                sender.Send += (o, e) => message = e.Message;

                sender.Publish("hallo");

                Assert.Equal("\"hallo\"", message);
            }
        }
    }
}
