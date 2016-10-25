using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class Class1
    {
        [Fact]
        public void SendMessageShouldBeReceived()
        {
            // Arrange
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                var person = new Person { FirstName = "Test", LastName = "Man" };
                var message = JsonConvert.SerializeObject(person);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
            }

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                using (var wait = new AutoResetEvent(false))
                {
                    Person person = null;

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        person = JsonConvert.DeserializeObject<Person>(message);
                        wait.Set();
                    };

                    channel.BasicConsume(queue: "hello",
                                         noAck: true,
                                         consumer: consumer);

                    Assert.True(wait.WaitOne(TimeSpan.FromSeconds(30)));
                    Assert.Equal(new Person { FirstName = "Test", LastName = "Man" }, person, new PersonComparer());
                }
            }
        }
    }
}
