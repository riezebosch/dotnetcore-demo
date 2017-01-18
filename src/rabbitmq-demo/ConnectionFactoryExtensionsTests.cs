using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rabbitmq_demo
{
    public static class ConnectionFactoryExtensions
    {
        public static ConnectionFactory FromEnvironment(this ConnectionFactory factory)
        {
            var host = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
            if (!string.IsNullOrWhiteSpace(host))
            {
                factory.HostName = host;
            }

            var user = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
            if (!string.IsNullOrWhiteSpace(user))
            {
                factory.UserName = user;
            }

            var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
            if (!string.IsNullOrWhiteSpace(password))
            {
                factory.Password = password;
            }

            return factory;
        }
    }
}
