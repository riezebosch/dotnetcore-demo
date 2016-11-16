using Autofac;
using Xunit;
using NSubstitute;

namespace rabbitmq_demo.tests
{

    public class AutofacBuilderExtensionsTests
    {
        [Fact]
        public void RegisterReceiverForContractInterface()
        {
            var builder = new ContainerBuilder();
            var dependency = Substitute.For<IDependency>();
            builder.RegisterInstance(dependency);

            builder.RegisterReceiverFor<ReceiverWithDependency, int>();
            var container = builder.Build();

            var receiver = container.Resolve<IReceive<int>>();
        }
    }
}
