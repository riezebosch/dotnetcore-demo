using catalogus_events.Mapping;
using catalogus_events.Model;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace catalogus_events
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string connection = ReadLineDefaultIfEmpty(askfor: "Connection string to Products database",
                otherwise: @"Data Source=.\SQLEXPRESS;Initial Catalog=Product;Integrated Security=SSPI");

            var host = ReadLineDefaultIfEmpty(
                askfor: "RabbitMQ host", 
                otherwise: "localhost");

            Console.Write("Namespace: ");
            var ns = Console.ReadLine();

            var user = ReadLineDefaultIfEmpty(
                askfor: "User", 
                otherwise: "guest");

            var password = ReadLineDefaultIfEmpty(
                askfor: "Password", 
                otherwise: "guest");

            var options = new DbContextOptionsBuilder<ProductContext>()
                .UseSqlServer(connection)
                .Options;

            try
            {
                using (var context = new ProductContext(options))
                using (var sender = new Sender(new ConnectionFactory { HostName = host, UserName = user, Password = password }, ns))
                {
                    sender.Send += (o, e) => Console.WriteLine(e);

                    var repository = new ProductRepository(context);
                    var products = repository.LoadProductenMetCategorieenEnLeverancier();
                    var mapper = EventMappers.CreateMapper();
                    var publisher = new ProductPublisher(mapper);

                    publisher.Publish(sender, products);
                }
            }
            catch (Exception ex)
            {
                var original = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.ToString());
                Console.ForegroundColor = original;
            }
        }

        private static string ReadLineDefaultIfEmpty(string askfor, string otherwise)
        {
            Console.Write($"{askfor} (empty for default):");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                input = otherwise;
                Console.WriteLine($"Using default: {input}");
            }

            Console.WriteLine();
            return input;
        }
    }
}
