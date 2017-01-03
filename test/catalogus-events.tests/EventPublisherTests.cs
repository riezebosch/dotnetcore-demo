using catalogus_events.Model;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace catalogus_events.tests
{
    public class EventPublisherTests
    {
        [Fact]
        public void Foo()
        {
            using (var receiver = new BlockingReceiver<ArtikelAanCatalogusToegevoegd>())
            using (var sender = new TestSender())
            using (var listener = sender.Listener())
            {
                receiver.SubscribeToEvents(listener);
                sender.PublishEvent(new ArtikelAanCatalogusToegevoegd());

                var m = receiver.Next();
            }
        }

        [Fact]
        public void ReadArtikelenFromDatabase()
        {
            using (var context = new ProductContext())
            {
                Assert.True(context.Product.Any());
            }
        }
    }
}
