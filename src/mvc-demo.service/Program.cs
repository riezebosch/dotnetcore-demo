using Microsoft.EntityFrameworkCore;
using mvc_demo.database;
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
        public static void Main()
        {
            using (var context = new FrontEndContext(
                new DbContextOptionsBuilder<FrontEndContext>()
                .UseSqlServer(@"Server=.\SQLEXPRESS;Database=mvc-demo;Trusted_Connection=true").Options))
            using (var receiver = new Listener(new ConnectionFactory { HostName = "curistm03", UserName = "manuel", Password = "manuel" }, "mvc-demo"))
            {
                context.Database.Migrate();

                var service = new FrontEndService(context);
                receiver.Subscribe(service);

                Console.ReadKey();
            }
        }
    }
}
