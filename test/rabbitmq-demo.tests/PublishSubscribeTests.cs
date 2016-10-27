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
                listener.Subscribe<Person>(person => output = person).ContinueWith(message => wait.Set());

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

        [Fact]
        public void ChainingFuncsWithContinuations()
        {
            var sb = new StringBuilder();
            var result = First.Do<string, StringBuilder>(m => sb.Append(m))
                .Then(builder => builder.ToString())
                .Then(s => s.ToUpper())
                .Invoke("m");

            Assert.Equal("M", result);
        }

        static class First
        {
            public static IInvoke<T, TResult> Do<T, TResult>(Func<T, TResult> a)
            {
                return new Invoker<T, TResult>(a);
            }
        }
    }

    internal class Invoker<T, TResult> : IInvoke<T, TResult>
    {
        private Func<T, TResult> a;

        public Invoker(Func<T, TResult> a)
        {
            this.a = a;
        }

        public TResult Invoke(T input)
        {
            return a(input);
        }

        public IInvoke<T, TNext> Then<TNext>(Func<T, TNext> p) where TNext : T
        {
            return new Invoker<T, TNext>(p);
        }
    }

    public interface IInvoke<T, TResult>
    {
        TResult Invoke(T input);
        IInvoke<T, TNext> Then<TNext>(Func<T, TNext> p) where TNext : T;
    }
}
