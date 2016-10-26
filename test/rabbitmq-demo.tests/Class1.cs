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
                listener.Subscribe<Person>(person =>
                {
                    output = person;
                    wait.Set();
                });

                Publish(input);
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(input, output, new PersonComparer());
            }
        }

        [Fact]
        public void SubsequentMessageShouldBeReceived()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Listener())
            {
                var people = new List<Person>();
                listener.Subscribe<Person>(person =>
                {
                    people.Add(person);
                    wait.Signal();
                });

                var first = new Person { FirstName = "first" };
                var second = new Person { FirstName = "second" };

                Publish(first);
                Publish(second);

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
                listener1.Subscribe<Person>(person =>
                {
                    people.Add(person);
                    wait.Signal();
                });

                listener2.Subscribe<Person>(person =>
                {
                    people.Add(person);
                    wait.Signal();
                });

                var messsage = new Person { FirstName = "first" };
                Publish(messsage);

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(new[] { messsage, messsage }, people, new PersonComparer());
            }
        }

        [Fact]
        public void OneListenerReceivesTwoMessagesOfDifferentType()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Listener())
            {
                listener.Subscribe<Person>(person => wait.Signal());
                listener.Subscribe<string>(message => wait.Signal());

                Publish(new Person { FirstName = "first" });
                Publish("just simple text");

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
            }
        }

        private static void Publish<T>(T input)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var exchange = typeof(T).FullName;
                channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout);

                var message = JsonConvert.SerializeObject(input);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: exchange,
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
            }
        }

        class Listener : IDisposable
        {
            IConnection connection;
            IModel channel;

            public Listener()
            {
                IConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
                connection = factory.CreateConnection();
                channel = connection.CreateModel();

               
            }

            public void Dispose()
            {
                connection.Dispose();
                channel.Dispose();
            }

            public void Subscribe<T>(Action<T> action)
            {
                var exchange = typeof(T).FullName;
                channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: exchange,
                                  routingKey: "");


                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    var item = JsonConvert.DeserializeObject<T>(message);
                    action(item);
                };

                channel.BasicConsume(queue: queueName,
                                 noAck: true,
                                 consumer: consumer);
            }
        }
    }
}
