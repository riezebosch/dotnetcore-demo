using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using NSubstitute;
using RabbitMQ.Client;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class MessageHandlerTests
    {
        [Fact]
        public void MessageHandlerDisposesFactoryCreatedReceivers()
        {
            // Arrange
            var service = Substitute.For<IReceive<int>, IDisposable>();
            var builder = new ContainerBuilder();
            builder.Register(c => service);

            using (var container = builder.Build())
            {
                var handler = new MessageReceivedHandler<int>(container, (c, m) => { });

                // Act
                handler.Handle(this, new RabbitMQ.Client.Events.BasicDeliverEventArgs { Body = 3.ToMessage().ToBody() });

                // Assert
                ((IDisposable)service).Received(1).Dispose();
            }
        }

        [Fact]
        public void MessageHandlerDisposesDependenciesResolvedToCreateInstances()
        {
            // Arrange
            var dependency = Substitute.For<IDependency, IDisposable>();

            var builder = new ContainerBuilder();
            builder.Register(c => dependency);

            builder
                .RegisterReceiverFor<ReceiverWithDependency, int>();

            using (var container = builder.Build())
            {
                var handler = new MessageReceivedHandler<int>(container, (t, m) => { });

                // Act
                handler.Handle(null, new RabbitMQ.Client.Events.BasicDeliverEventArgs { Body = 3.ToMessage().ToBody() });

                // Assert
                ((IDisposable)dependency).Received(1).Dispose();
            }
        }

        [Fact]
        public void MessageHandlerResolvesDependenciesToCreateInstances()
        {
            // Arrange
            var dependency = Substitute.For<IDependency>();
            var builder = new ContainerBuilder();
            builder
                .RegisterInstance(dependency);
            builder
                .RegisterReceiverFor<ReceiverWithDependency, int>();

            using (var container = builder.Build())
            {
                var handler = new MessageReceivedHandler<int>(container, (t, m) => { });

                // Act
                handler.Handle(null, new RabbitMQ.Client.Events.BasicDeliverEventArgs { Body = 3.ToMessage().ToBody() });

                // Assert
                dependency.Received(1).Foo();
            }
        }

    }
}
