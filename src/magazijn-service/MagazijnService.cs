using System;
using rabbitmq_demo;
using magazijn_service.Commands;
using magazijn_service.Events;
using magazijn_service.Model;

namespace magazijn_service
{
    public class MagazijnService : 
        IReceive<ZetArtikelInMagazijn>,
        IReceive<ArtikelAanCatalogusToegevoegd>,
        IReceive<HaalArtikelUitMagazijn>
    {
        private VoorraadContext _context;
        private ISender _sender;

        public MagazijnService(ISender sender, VoorraadContext context)
        {
            this._sender = sender;
            this._context = context;
        }

        public void Execute(HaalArtikelUitMagazijn item)
        {
            var artikel = _context.Artikelen.Find(item.ArtikelId);
            if (artikel != null)
            {
                artikel.Voorraad--;
                _context.SaveChanges();

                _sender.PublishEvent(new ArtikelVoorraadBijgewerkt { Id = artikel.Id, Voorraad = artikel.Voorraad });
            }
            else
            {
                _sender.PublishEvent(new ArtikelVoorraadBijgewerkt { Id = item.ArtikelId, Voorraad = 0 });
            }
        }

        public void Execute(ArtikelAanCatalogusToegevoegd item)
        {
            var artikel = new Artikel { Id = item.Id, Voorraad = 2 };
            _context.Artikelen.Add(artikel);
            _context.SaveChanges();

            _sender.PublishEvent(new ArtikelVoorraadBijgewerkt { Id = artikel.Id, Voorraad = artikel.Voorraad });
        }

        public void Execute(ZetArtikelInMagazijn item)
        {
            var artikel = _context.Artikelen.Find(item.ArtikelId);
            if (artikel == null)
            {
                artikel = new Artikel();
                _context.Artikelen.Add(artikel);
            }

            artikel.Voorraad++;
            _context.SaveChanges();

            _sender.PublishEvent(new ArtikelVoorraadBijgewerkt { Id = artikel.Id, Voorraad = artikel.Voorraad });
        }
    }
}