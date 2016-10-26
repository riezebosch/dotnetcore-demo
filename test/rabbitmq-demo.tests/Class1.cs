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
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var wait = new ManualResetEvent(false))
            using (var listener = new Listener())
            {
                Person output = null;

                // Act
                listener.Received += (o, person) =>
                {
                    output = person;
                    wait.Set();
                };

                Send(input);
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(input, output, new PersonComparer());
            }
        }

        [Fact]
        public void SubsequentSecondMessageShouldBeReceived()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Listener())
            {
                var people = new List<Person>();
                listener.Received += (o, person) =>
                {
                    people.Add(person);
                    wait.Signal();
                };

                var first = new Person { FirstName = "first" };
                var second = new Person { FirstName = "second" };

                Send(first);
                Send(second);

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(new[] { first, second }, people, new PersonComparer());
            }
        }

        private static void Send(Person input)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                var message = JsonConvert.SerializeObject(input);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
            }
        }

        class Listener : IDisposable
        {
            IConnection connection;
            IModel channel;

            public event EventHandler<Person> Received;

            public Listener()
            {
                IConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                {
                    channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                }

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    var person = JsonConvert.DeserializeObject<Person>(message);
                    Received?.Invoke(this, person);
                };

                channel.BasicConsume(queue: "hello",
                                 noAck: true,
                                 consumer: consumer);
            }

            
            public void Dispose()
            {
                connection.Dispose();
                channel.Dispose();
            }
        }
    }
}
