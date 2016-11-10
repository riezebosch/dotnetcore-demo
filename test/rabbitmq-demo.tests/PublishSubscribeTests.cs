using Autofac;
using ef_demo;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using rabbitmq_demo.tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class PublishSubscribeTests
    {
        [Fact]
        public void PublishMessageShouldBeReceivedBySubsriber()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var wait = new ManualResetEvent(false))
            using (var listener = new Listener())
            {
                var mock = new Mock<IReceive<Person>>();
                mock
                    .Setup(m => m.Execute(
                        It.Is<Person>(p => p.FirstName == "Test" && p.LastName == "Man")))
                    .Callback(() => wait.Set())
                    .Verifiable();

                // Act
                listener
                    .Subscribe<Person>(mock.Object);

                using (var sender = new Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                mock.Verify();
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

            using (var listener = new Listener(connection, "demo"))
            {
                var receive = new ReceiveAsync<Person>();
                listener.Subscribe(receive);

                // Act
                using (var sender = new Sender(connection, "demo"))
                {
                    sender.Publish(input);
                }

                // Assert
                await receive.WithTimeout(TimeSpan.FromSeconds(5));
            }
        }


        [Fact]
        public void SubsequentMessageShouldBeReceivedBySubscriber()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Listener())
            {
                var mock = new Mock<IReceive<Person>>();
                mock
                    .Setup(m => m.Execute(
                        It.Is<Person>(p => p.FirstName == "first")))
                    .Callback(() => wait.Signal())
                    .Verifiable();

                mock
                    .Setup(m => m.Execute(
                        It.Is<Person>(p => p.FirstName == "second")))
                    .Callback(() => wait.Signal())
                    .Verifiable();

                listener.Subscribe(mock.Object);

                using (var sender = new Sender())
                {
                    sender.Publish(new Person { FirstName = "first" });
                    sender.Publish(new Person { FirstName = "second" });
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                mock.Verify();
            }
        }

        [Fact]
        public void TwoListenersBothReceiveMessageAfterPublish()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener1 = new Listener())
            using (var listener2 = new Listener())
            {
                var mock = new Mock<IReceive<Person>>();
                mock
                    .Setup(m => m.Execute(It.IsAny<Person>()))
                    .Callback(() => wait.Signal());

                listener1.Subscribe(mock.Object);
                listener2.Subscribe(mock.Object);

                using (var sender = new Sender())
                {
                    sender.Publish(new Person { FirstName = "first" });
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                mock.Verify(m => m.Execute(It.IsAny<Person>()), Times.Exactly(2));
            }
        }

        [Fact]
        public void OneListenerReceivesTwoMessagesOfDifferentType()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Listener())
            {
                var mock1 = new Mock<IReceive<Person>>();
                mock1
                    .Setup(m => m.Execute(It.IsAny<Person>()))
                    .Callback(() => wait.Signal())
                    .Verifiable();

                var mock2 = new Mock<IReceive<string>>();
                mock2
                    .Setup(m => m.Execute(It.IsAny<string>()))
                    .Callback(() => wait.Signal())
                    .Verifiable();

                listener.Subscribe(mock1.Object);
                listener.Subscribe(mock2.Object);

                using (var sender = new Sender())
                {
                    sender.Publish(new Person { FirstName = "first" });
                    sender.Publish("just simple text");
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                mock1.Verify();
                mock2.Verify();
            }
        }

        [Fact]
        public void SendAndReceiveShouldNotDependOnClrTypes()
        {
            // Arrange
            var input = new ef_demo.Person { FirstName = "Test", LastName = "Man" };

            using (var wait = new ManualResetEvent(false))
            using (var listener = new Listener())
            {
                var mock = new Mock<IReceive<Person>>();
                mock
                    .Setup(m => m.Execute(
                        It.Is<rabbitmq_demo.tests.Person>(p =>
                            p.FirstName == input.FirstName
                            && p.LastName == input.LastName)))
                    .Callback(() => wait.Set());

                listener.Subscribe(mock.Object);

                // Act
                using (var sender = new Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                mock.Verify();
            }
        }

        [Fact]
        public void SendAndReceiveShouldDependOnClassName()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var listener = new Listener())
            using (var wait = new ManualResetEvent(false))
            {
                var mock = new Mock<IReceive<SomethingUnrelated>>();
                mock.Setup(m => m.Execute(It.IsAny<SomethingUnrelated>())).Callback(() => wait.Set());
                listener.Subscribe(mock.Object);

                // Act
                using (var sender = new Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                Assert.False(wait.WaitOne(TimeSpan.FromSeconds(1)));
                mock.Verify(m => m.Execute(It.IsAny<SomethingUnrelated>()), Times.Never);
            }
        }

        [Fact]
        public async Task WaitForResult()
        {
            using (var listener = new Listener())
            using (var sender = new Sender())
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
            using (var sender = new Sender())
            {
                var receiver = new ReceiveAsync<int>();
                listener.Subscribe(receiver);

                await Assert.ThrowsAsync<TimeoutException>(() => receiver.WithTimeout(TimeSpan.FromSeconds(1)));
            }
        }

        [Fact]  
        public async Task ListenerRaisesEvent()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
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
        public async Task ListenerUsesFactoryToCreateInstances()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var repository = new MockRepository(MockBehavior.Strict);
                listener
                    .Subscribe(() =>
                    {
                        var mock = repository.Create<IReceive<int>>();
                        mock.Setup(m => m.Execute(3)).Verifiable();

                        return mock.Object;
                    });

                var waiter = new ReceiveAsync<int>();
                listener.Subscribe(waiter);

                // Act
                sender.Publish(3);
                await waiter.WithTimeout();

                // Assert
                repository.Verify();
            }
        }

        [Fact]
        public async Task ListenerDoesNotDisposeInstanceSubscribedReceivers()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var mock = new Mock<IDisposable>();
                listener.Subscribe(mock.As<IReceive<int>>().Object);

                var waiter = new ReceiveAsync<int>();
                listener.Subscribe(waiter);
                
                // Act
                sender.Publish(3);
                await waiter.WithTimeout();

                // Assert
                mock.Verify(m => m.Dispose(), Times.Never);
            }
        }

        [Fact]
        public async Task ListenerDisposesFactoryCreatedReceivers()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var reposistory = new MockRepository(MockBehavior.Strict);
                listener.Subscribe(() =>
                {
                    var mock = reposistory.Create<IReceive<int>>();
                    mock
                        .As<IDisposable>()
                        .Setup(m => m.Dispose())
                        .Verifiable();

                    return mock.Object;
                });

                var waiter = new ReceiveAsync<int>();
                listener.Subscribe(waiter);

                // Act
                sender.Publish(3);
                await waiter.WithTimeout();

                // Assert
                reposistory.Verify();
            }
        }

        [Fact]
        public async Task ListenerResolvesDependenciesToCreateInstances()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var mock = new Mock<IDependency>();
                mock.Setup(m => m.Foo());

                var builder = new ContainerBuilder();
                builder.RegisterInstance(mock.Object);
                builder.RegisterType<ReceiverWithDependency>().As<IReceive<int>>();

                listener.Subscribe<int>(builder.Build());

                var waiter = new ReceiveAsync<int>();
                listener.Subscribe(waiter);

                // Act
                sender.Publish(3);
                await waiter.WithTimeout();

                // Assert
                mock.Verify(m => m.Foo(), Times.Once);
            }
        }

        [Fact]
        public async Task ListenerDisposesDependenciesResolvedToCreateInstances()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var repository = new MockRepository(MockBehavior.Strict);

                var builder = new ContainerBuilder();
                builder.Register(c =>
                {
                    var mock = repository.Create<IDependency>();
                    mock.Setup(x => x.Foo())
                        .Verifiable();
                    mock.As<IDisposable>()
                        .Setup(x => x.Dispose())
                        .Verifiable();

                    return mock.Object;
                });

                builder
                    .RegisterType<ReceiverWithDependency>()
                    .As<IReceive<int>>();

                listener.Subscribe<int>(builder.Build());

                var waiter = new ReceiveAsync<int>();
                listener.Subscribe(waiter);

                // Act
                sender.Publish(3);
                await waiter.WithTimeout();

                // Assert
                repository.VerifyAll();
            }
        }

        public interface IDependency
        {
            void Foo();
        }

        class ReceiverWithDependency : IReceive<int>
        {
            private readonly IDependency _dependency;

            public ReceiverWithDependency(IDependency dependency)
            {
                _dependency = dependency;
            }
            public void Execute(int item)
            {
                _dependency.Foo();
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
