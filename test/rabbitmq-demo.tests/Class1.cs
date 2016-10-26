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

        [Fact]
        public void TwoListenersBothReceiveMessageFromQueue()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener1 = new Listener())
            using (var listener2 = new Listener())
            {
                var people = new List<Person>();
                listener1.Received += (o, person) =>
                {
                    people.Add(person);
                    wait.Signal();
                };

                listener2.Received += (o, person) =>
                {
                    people.Add(person);
                    wait.Signal();
                };

                var messsage = new Person { FirstName = "first" };

                Send(messsage);

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(new[] { messsage, messsage }, people, new PersonComparer());
            }
        }

        private static void Send(Person input)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                var message = JsonConvert.SerializeObject(input);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "logs",
                                     routingKey: "",
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

                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: "logs",
                                  routingKey: "");


                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    var person = JsonConvert.DeserializeObject<Person>(message);
                    Received?.Invoke(this, person);
                };

                channel.BasicConsume(queue: queueName,
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
