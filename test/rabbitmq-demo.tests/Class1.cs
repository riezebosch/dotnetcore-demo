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
        public void PublishMessageShouldBeReceivedBySubsriber()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var wait = new ManualResetEvent(false))
            using (var listener = new Receiver())
            {
                Person output = null;

                // Act
                listener.Subscribe<Person>(person =>
                {
                    output = person;
                    wait.Set();
                });

                using (var sender = new Sender())
                {
                    sender.Publish(input);
                }

                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(input, output, new PersonComparer());
            }
        }

        [Fact]
        public void SubsequentMessageShouldBeReceivedBySubscriber()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Receiver())
            {
                var people = new List<Person>();
                listener.Subscribe<Person>(person =>
                {
                    people.Add(person);
                    wait.Signal();
                });

                var first = new Person { FirstName = "first" };
                var second = new Person { FirstName = "second" };

                using (var sender = new Sender())
                {
                    sender.Publish(first);
                    sender.Publish(second);
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(new[] { first, second }, people, new PersonComparer());
            }
        }

        [Fact]
        public void TwoListenersBothReceiveMessageAfterPublish()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener1 = new Receiver())
            using (var listener2 = new Receiver())
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
                using (var sender = new Sender())
                {
                    sender.Publish(messsage);
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(new[] { messsage, messsage }, people, new PersonComparer());
            }
        }

        [Fact]
        public void OneListenerReceivesTwoMessagesOfDifferentType()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Receiver())
            {
                listener.Subscribe<Person>(person => wait.Signal());
                listener.Subscribe<string>(message => wait.Signal());

                using (var sender = new Sender())
                {
                    sender.Publish(new Person { FirstName = "first" });
                    sender.Publish("just simple text");
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
            }
        }

        class Sender : IDisposable
        {
            private readonly string _hostname;
            private readonly string _exchange;

            IConnection _connection;
            IModel _channel;

            public Sender(string hostname = "localhost", string exchange = "demo")
            {
                _hostname = hostname;
                _exchange = exchange;

                var factory = new ConnectionFactory() { HostName = _hostname };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout);
            }

            public void Publish<T>(T input)
            {
                var routingKey = typeof(T).FullName;

                var message = JsonConvert.SerializeObject(input);
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: _exchange,
                                     routingKey: routingKey,
                                     basicProperties: null,
                                     body: body);
            }

            public void Dispose()
            {
                _connection.Dispose();
                _channel.Dispose();
            }
        }

        class Receiver : IDisposable
        {
            private readonly string _hostname;
            private readonly string _exchange;

            IConnection connection;
            IModel channel;

            public Receiver(string hostname = "localhost", string exchange = "demo")
            {
                _hostname = hostname;
                _exchange = exchange;

                IConnectionFactory factory = new ConnectionFactory() { HostName = _hostname };
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
                var routingkey = typeof(T).FullName;
                channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: _exchange,
                                  routingKey: routingkey);


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
