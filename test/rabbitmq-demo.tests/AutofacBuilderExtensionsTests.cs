using Autofac;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    
    public class AutofacBuilderExtensionsTests
    {
        [Fact]
        public void RegisterReceiverForContractInterface()
        {
            var builder = new ContainerBuilder();
            {
                var dependency = new Mock<IDependency>();
                builder.RegisterInstance(dependency.Object);

                builder.RegisterReceiverFor<ReceiverWithDependency, int>();
                var container = builder.Build();

                var receiver = container.Resolve<IReceive<int>>();
            }
        }
    }
}
