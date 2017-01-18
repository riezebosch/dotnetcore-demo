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

            var factory = new ConnectionFactory()
                .FromEnvironment();

            var builder = new ContainerBuilder();
            builder
                .RegisterReceiverFor<MagazijnService, ZetArtikelInMagazijn>();

            builder.Register(c => 
                    new VoorraadContext(new DbContextOptionsBuilder<VoorraadContext>()
                        .UseSqlite("File=./data/voorraad.db")
                        .Options));

            builder
                .Register<ISender>(c =>
                    new Sender(factory, "Kantilever"));

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
