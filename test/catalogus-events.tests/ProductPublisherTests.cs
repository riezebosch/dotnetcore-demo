using AutoMapper;
using catalogus_events.Events;
using catalogus_events.Mapping;
using catalogus_events.Model;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace catalogus_events.tests
{
    public class ProductPublisherTests
    {
        [Fact]
        public void PublishArtikelAanCatalogusToegevoegdEventForAllProducten()
        {
            var producten = new List<Product>
            {
                new Product { Naam = "Fiets" },
            };

            using (var receiver = new BlockingReceiver<ArtikelAanCatalogusToegevoegd>())
            using (var sender = new TestSender())
            using (var listener = sender.Listener())
            {
                receiver.SubscribeToEvents(listener);

                var mapper = EventMappers.CreateMapper();
                var publisher = new ProductPublisher(mapper);

                publisher.Publish(sender, producten);

                var m = receiver.Next();
                Assert.Equal("Fiets", m.Naam);
            }
        }
    }
}
