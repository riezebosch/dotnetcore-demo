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
        public static void Main()
        {
            var connection = new ConnectionFactory
            {
                HostName = "curistm01",
                UserName = "manuel",
                Password = "manuel"
            };

            using (var receiver = new Listener(connection, "rabbitmq-demo"))
            using (var sender = new Sender(connection, exchange: "rabbitmq-demo"))
            {
                receiver.Subscribe(new WriteLine());

                string input = string.Empty;
                while ((input = Console.ReadLine()) != "exit")
                {
                    sender.Publish(input);
                }
            }
        }

        private class WriteLine : IReceive<string>
        {
            public void Execute(string item)
            {
                Console.WriteLine(item);
            }
        }
    }
}
