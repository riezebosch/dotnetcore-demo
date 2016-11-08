using RabbitMQ.Client;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rabbitmq_console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connection = new ConnectionFactory
            {
                HostName = "curistm01",
                UserName = "manuel",
                Password = "manuel"
            };

            using (var receiver = new Receiver(connection, "rabbitmq-demo"))
            using (var sender = new Sender(connection, exchange: "rabbitmq-demo"))
            {
                receiver.Subscribe<string>(Console.WriteLine);

                string input = string.Empty;
                while ((input = Console.ReadLine()) != "exit")
                {
                    sender.Publish(input);
                }
            }
        }
    }
}
