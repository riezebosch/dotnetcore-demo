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
    public class ListenerTests
    {
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
                Assert.Throws<ComponentNotRegisteredException>(() => listener.Subscribe<int>(builder.Build()));
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
                Assert.Throws<DependencyResolutionException>(() => listener.Subscribe<int>(builder.Build()));
            }
        }

        [Fact]
        public void ListenerDisposesDependenciesResolvedToCreateInstances()
        {
            // Arrange
            using (var listener = new TestListener())
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
    }
}
