using ef_demo;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using rabbitmq_demo.tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace rabbitmq_demo.tests
{
    public class PublishSubscribeTests
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
                listener
                    .Subscribe<Person>(p =>
                    {
                        output = p;
                        wait.Set();
                    });

                using (var sender = new Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.False(ReferenceEquals(input, output));
                Assert.Equal(input, output, new PersonComparer());
            }
        }

        [Fact]
        public void ConnectWithCredentials()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };
            var connection = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using (var wait = new ManualResetEvent(false))
            using (var listener = new Receiver(connection, "demo"))
            {
                // Act
                listener.Subscribe<Person>(p => wait.Set());
                using (var sender = new Sender(connection, "demo"))
                {
                    sender.Publish(input);
                }

                // Assert
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
            }
        }

        [Fact]
        public void SubsequentMessageShouldBeReceivedBySubscriber()
        {
            using (var wait = new CountdownEvent(2))
            using (var listener = new Receiver())
            {
                var people = new List<Person>();
                listener
                    .Subscribe<Person>(p =>
                    {
                        people.Add(p);
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
                listener1
                    .Subscribe<Person>(p =>
                    {
                        people.Add(p);
                        wait.Signal();
                    });
                listener2
                    .Subscribe<Person>(p =>
                    {
                        people.Add(p);
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
                listener.Subscribe<Person>(p => wait.Signal());
                listener.Subscribe<string>(p => wait.Signal());

                using (var sender = new Sender())
                {
                    sender.Publish(new Person { FirstName = "first" });
                    sender.Publish("just simple text");
                }

                Assert.True(wait.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
            }
        }

        [Fact]
        public void SendAndReceiveShouldNotDependOnClrTypes()
        {
            // Arrange
            var input = new ef_demo.Person { FirstName = "Test", LastName = "Man" };

            using (var wait = new ManualResetEvent(false))
            using (var listener = new Receiver())
            {
                rabbitmq_demo.tests.Person output = null;

                // Act
                listener
                    .Subscribe<rabbitmq_demo.tests.Person>(p =>
                    {
                        output = p;
                        wait.Set();
                    });

                using (var sender = new Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                Assert.True(wait.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.Equal(input.FirstName, output.FirstName);
            }
        }

        [Fact]
        public void SendAndReceiveShouldDependOnClassName()
        {
            // Arrange
            var input = new Person { FirstName = "Test", LastName = "Man" };

            using (var wait = new ManualResetEvent(false))
            using (var listener = new Receiver())
            {
                SomethingUnrelated output = null;

                // Act
                listener
                    .Subscribe<SomethingUnrelated>(p =>
                    {
                        output = p;
                        wait.Set();
                    });

                using (var sender = new Sender())
                {
                    sender.Publish(input);
                }

                // Assert
                Assert.False(wait.WaitOne(TimeSpan.FromSeconds(1)));
            }
        }

        [Fact]
        public void WaitForResult()
        {
            using (var receiver = new Receiver())
            using (var sender = new Sender())
            {
                var result = receiver.WaitForResult<int>(() => sender.Publish(3));
                Assert.Equal(3, result);
            }
        }

        [Fact]
        public void WaitForResultTimeoutWhenNotReceiving()
        {
            using (var receiver = new Receiver())
            using (var sender = new Sender())
            {
                Assert.Throws<TimeoutException>(() => receiver.WaitForResult<int>(() => { }, TimeSpan.FromSeconds(1)));
            }
        }
    }
}
