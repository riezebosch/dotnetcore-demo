using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rabbitmq_demo;
using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using magazijn_service.Commands;
using magazijn_service.Model;

namespace magazijn_service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Magazijn";

            var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            var user = Environment.GetEnvironmentVariable("RABBITMQ_USER");
            var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = password
            };

            var builder = new ContainerBuilder();
            builder
                .RegisterReceiverFor<MagazijnService, ZetArtikelInMagazijn>();

            builder
                .Register<VoorraadContext>(c => 
                new VoorraadContext(new DbContextOptionsBuilder<VoorraadContext>()
                    .UseSqlite("File=./data/voorraad.db")
                    .Options));

            builder
                .Register<ISender>(c => 
                    new Sender(factory, "kantilever"));

            using (var container = builder.Build())
            using (var listener = new Listener(factory, "Kantilever"))
            using (var are = new AutoResetEvent(false))
            {
                listener.Received += (o, e) => Console.WriteLine(e);
                listener.Exceptions += (o, e) => Console.WriteLine(e);

                listener.SubscribeEvents<ZetArtikelInMagazijn>(container);

                Console.WriteLine("service started");
                are.WaitOne();
            }
        }
    }
}
