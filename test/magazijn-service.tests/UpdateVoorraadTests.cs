using magazijn_service.Commands;
using magazijn_service.Events;
using magazijn_service.Model;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using rabbitmq_demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace magazijn_service.tests
{
    public class UpdateVoorraadTests
    {
        [Fact]
        public void ArtikelInMagazijnGezetWerktVoorraadBij()
        {
            var sender = Substitute.For<ISender>();
            using (var context = InitializeTestContext())
            {
                var artikel = AddTestArtikel(context);

                var service = new MagazijnService(sender, context);

                service.Execute(new ZetArtikelInMagazijn { ArtikelId = artikel.Id });
                sender
                    .Received(1)
                    .PublishEvent(Arg.Is<ArtikelVoorraadBijgewerkt>(a => a.Id == artikel.Id && a.Voorraad == 6));

                Assert.Equal(6, artikel.Voorraad);
                Assert.DoesNotContain(context.ChangeTracker.Entries(), c => c.State != EntityState.Unchanged);
            }
        }

        [Fact]
        public void ArtikelUitMagazijnGehaaldWerktVoorraadBij()
        {
            var sender = Substitute.For<ISender>();
            using (var context = InitializeTestContext())
            {
                var artikel = AddTestArtikel(context);
                var service = new MagazijnService(sender, context);

                service.Execute(new HaalArtikelUitMagazijn { ArtikelId = artikel.Id });

                sender
                    .Received(1)
                    .PublishEvent(Arg.Is<ArtikelVoorraadBijgewerkt>(a => a.Id == artikel.Id && a.Voorraad == 4));

                Assert.Equal(4, artikel.Voorraad);
                Assert.DoesNotContain(context.ChangeTracker.Entries(), c => c.State != EntityState.Unchanged);
            }
        }

        [Fact]
        public void ArtikelAanCatalogusToegevoegdZorgtVoorVoorraad()
        {
            var sender = Substitute.For<ISender>();
            using (var context = InitializeTestContext())
            {
                var service = new MagazijnService(sender, context);
                service.Execute(new ArtikelAanCatalogusToegevoegd { Id = 1 });

                sender
                    .Received(1)
                    .PublishEvent(Arg.Is<ArtikelVoorraadBijgewerkt>(a => a.Id == 1 && a.Voorraad == 2));

                var artikel = context.Artikelen.Find(1);
                Assert.NotNull(artikel);
                Assert.Equal(2, artikel.Voorraad);

                Assert
                    .DoesNotContain(context.ChangeTracker.Entries(), c => c.State != EntityState.Unchanged);
            }
        }

        [Fact]
        public void NietBestaanArtikelWordtToegevoegdBijInMagazijnZetten()
        {
            var sender = Substitute.For<ISender>();
            using (var context = InitializeTestContext())
            {
                var service = new MagazijnService(sender, context);
                service.Execute(new ZetArtikelInMagazijn { ArtikelId = 1 });

                var artikel = context.Artikelen.Find(1);
                Assert.NotNull(artikel);
                Assert.Equal(1, artikel.Voorraad);
            }
        }

        [Fact]
        public void NietBestaanArtikelWordtWelDoorgegooidBijUitMagazijnHalen()
        {
            var sender = Substitute.For<ISender>();
            using (var context = InitializeTestContext())
            {
                var service = new MagazijnService(sender, context);
                service.Execute(new HaalArtikelUitMagazijn { ArtikelId = 1 });

                sender
                   .Received(1)
                   .PublishEvent(Arg.Is<ArtikelVoorraadBijgewerkt>(a => a.Id == 1 && a.Voorraad == 0));
            }
        }

        private static VoorraadContext InitializeTestContext()
        {
            return new VoorraadContext(
                new DbContextOptionsBuilder<VoorraadContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options);
        }

        private static Artikel AddTestArtikel(VoorraadContext context)
        {
            var artikel = new Artikel { Id = 1, Voorraad = 5 };
            context.Artikelen.Add(artikel);
            context.SaveChanges();

            return artikel;
        }
    }
}
