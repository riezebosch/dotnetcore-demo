﻿using FirstThen;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabbitmq_demo
{
    public class Receiver : IDisposable
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

        public IDo<T, T> Subscribe<T>()
        {
            var routingkey = typeof(T).FullName;
            channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout);

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName,
                              exchange: _exchange,
                              routingKey: routingkey);


            var consumer = new EventingBasicConsumer(channel);

            var result = First.Do<T, T>(m => m);

            consumer.Received += (model, ea) =>
            {
                result.Invoke(First
                    .Do<byte[], string>(Encoding.UTF8.GetString)
                    .Then(JsonConvert.DeserializeObject<T>)
                    .Finally()
                    .Execute(ea.Body));
            };

            channel.BasicConsume(queue: queueName,
                             noAck: true,
                             consumer: consumer);

            return result;
        }
    }
}
