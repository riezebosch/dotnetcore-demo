using RabbitMQ.Client;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            using (var listener = new Listener(connection, "rabbitmq-demo"))
            using (var sender = new Sender(connection, exchange: "rabbitmq-demo"))
            {
                listener.Received += Print;
                listener.Subscribe(new WriteLine());

                string input = string.Empty;
                while ((input = Console.ReadLine()) != "exit")
                {
                    sender.Publish(input);
                }
            }
        }

        private static void Print(object sender, ReceivedEventArgs e)
        {
            WriteLineColor(e.ToString(), ConsoleColor.DarkGray);
        }

        private class WriteLine : IReceive<string>
        {
            public void Execute(string item)
            {
                WriteLineColor(item, ConsoleColor.Green);
            }

        }

        private static void WriteLineColor(string text, ConsoleColor color)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = color;

            try
            {
                Console.WriteLine(text);
            }
            finally
            {
                Console.ForegroundColor = original;
            }
        }

    }
}
