using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo
{
    public class ConnectionFactoryExtensionsTests
    {
        [Fact]
        public void ConfigureHostNameFromEnvironmentVariables()
        {
            var value = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable("RABBITMQ_HOSTNAME", value);

            var factory = new ConnectionFactory().FromEnvironment();
            Assert.Equal(value, factory.HostName);
        }

        [Fact]
        public void DontConfigureHostNameWhenEnvironmentVariableEmpty()
        {
            var factory = new ConnectionFactory();
            var value = factory.HostName;

            Environment.SetEnvironmentVariable("RABBITMQ_HOSTNAME", "");
            factory = factory.FromEnvironment();

            Assert.Equal(value, factory.HostName);
        }

        [Fact]
        public void ConfigureUserNameFromEnvironmentVariables()
        {
            var value = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", value);

            var factory = new ConnectionFactory().FromEnvironment();
            Assert.Equal(value, factory.UserName);
        }

        [Fact]
        public void DontConfigureUserNameWhenEnvironmentVariableEmpty()
        {
            var factory = new ConnectionFactory();
            var value = factory.UserName;

            Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "");
            factory = factory.FromEnvironment();

            Assert.Equal(value, factory.UserName);
        }

        [Fact]
        public void ConfigurePasswordFromEnvironmentVariables()
        {
            var value = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", value);

            var factory = new ConnectionFactory().FromEnvironment();
            Assert.Equal(value, factory.Password);
        }

        [Fact]
        public void DontConfigurePasswordWhenEnvironmentVariableEmpty()
        {
            var factory = new ConnectionFactory();
            var value = factory.Password;

            Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "");
            factory = factory.FromEnvironment();

            Assert.Equal(value, factory.Password);
        }
    }
}
