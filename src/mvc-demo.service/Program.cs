using RabbitMQ.Client;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mvc_demo.service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connection = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using (var receiver = new Receiver(connection, "rabbitmq-services"))
            {
                receiver.Subscribe<PersonCreated>(p => { });
                Console.ReadKey();
            }
        }
    }
}
