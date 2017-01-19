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
            string connection = 
                Environment.GetEnvironmentVariable("CONNECTIONSTRING");

            var options = new DbContextOptionsBuilder<ProductContext>()
                .UseSqlServer(connection)
                .Options;

            try
            {
                using (var context = new ProductContext(options))
                using (var sender = new Sender(
                    new ConnectionFactory()
                    .FromEnvironment(), "Kantilever"))
                {
                    context.Database.Migrate();
                    sender.Send += (o, e) => Console.WriteLine(e);

                    var repository = 
                        new ProductRepository(context);

                    var products = 
                        repository.LoadProductenMetCategorieenEnLeverancier();

                    var mapper = 
                        EventMappers.CreateMapper();

                    var publisher = 
                        new ProductPublisher(mapper);

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
    }
}
