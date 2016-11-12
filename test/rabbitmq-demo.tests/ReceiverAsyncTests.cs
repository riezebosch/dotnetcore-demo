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
                //var service = new Mock<IReceive<int>>();
                var service = Substitute.For<IReceive<int>>();
                
                //service.Setup(m => m.Execute(3)).Verifiable();
                listener
                    .Subscribe(() => service);

                var waiter = new ReceiveAsync<int>();
                listener.Subscribe(waiter);

                // Act
                sender.Publish(3);
                await waiter.WithTimeout();

                // Assert
                service.Received().Execute(3);
            }
        }

        [Fact]
        public async Task ListenerDoesNotDisposeInstanceSubscribedReceivers()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var service = Substitute.For<IReceive<int>, IDisposable>();
                listener
                    .Subscribe(service);

                var waiter = new ReceiveAsync<int>();
                listener.Subscribe(waiter);
                
                // Act
                sender.Publish(3);
                await waiter.WithTimeout();

                ((IDisposable)service).DidNotReceive().Dispose();
            }
        }

        [Fact]
        public void ListenerDisposesFactoryCreatedReceivers()
        {
            // Arrange
            using (var listener = new Listener())
            {
                var service = Substitute.For<IReceive<int>, IDisposable>();

                // Act
                listener.Subscribe(() => service);

                // Assert
                ((IDisposable)service).Received(1).Dispose();
            }
        }

        [Fact]
        public async Task ListenerResolvesDependenciesToCreateInstances()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
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
        public void ListenerThrowsExceptionWhenReceiverForContractIsNotResolved()
        {
            // Arrange
            using (var listener = new Listener())
            using (var sender = new Sender())
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterType<ReceiverWithDependency>();

                // Act && Assert
                Assert.Throws<ComponentNotRegisteredException>(() => listener.Subscribe<int>(builder.Build()));
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
