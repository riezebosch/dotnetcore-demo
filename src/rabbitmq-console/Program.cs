using Autofac;
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
        private class CreatePerson
        {
            public string FirstName { get; set; }
        }

        public static void Main()
        {
            var connection = new ConnectionFactory
            {
                HostName = "curistm01",
                UserName = "manuel",
                Password = "manuel"
            };

            using (var listener = new Listener(connection, "DeelnemerService"))
            using (var sender = new Sender(connection, ns: "DeelnemerService"))
            {
                listener.Received += Print;
                sender.Send += Print;

                var builder = new ContainerBuilder();
                builder
                    .RegisterReceiverFor<WriteLine<CreatePerson>, CreatePerson>();

                using (var container = builder.Build())
                {
                    listener.SubscribeEvents<CreatePerson>(container);

                    string input = string.Empty;
                    while ((input = Console.ReadLine()) != "exit")
                    {
                        sender.Publish(new CreatePerson { FirstName = input });
                    }
                }
            }
        }

        private static void Print(object sender, EventArgs e)
        {
            WriteLineColor($@"{e.GetType().Name}
  {e.ToString()}", ConsoleColor.DarkGray);
        }

        private class WriteLine<T> : IReceive<T>
        {
            public void Execute(T item)
            {
                WriteLineColor(item.ToString(), ConsoleColor.Green);
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
